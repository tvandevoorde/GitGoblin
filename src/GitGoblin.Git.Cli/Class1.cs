using System.Diagnostics;
using GitGoblin.Core;
using GitGoblin.Git.Abstractions;

namespace GitGoblin.Git.Cli;

public sealed class GitCliRepositoryService : IGitRepositoryService
{
	public async Task<RepositoryStatus> GetStatusAsync(string repositoryPath, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(repositoryPath))
		{
			throw new ArgumentException("Repository path is required.", nameof(repositoryPath));
		}

		if (!Directory.Exists(repositoryPath))
		{
			throw new DirectoryNotFoundException($"Repository path does not exist: {repositoryPath}");
		}

		string output = await RunGitAsync(repositoryPath, "status --porcelain=v2 --branch", cancellationToken);
		return ParseStatus(repositoryPath, output);
	}

	private static RepositoryStatus ParseStatus(string repositoryPath, string output)
	{
		string branch = "(detached)";
		int ahead = 0;
		int behind = 0;
		List<GitFileChange> changes = [];

		string[] lines = output
			.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		foreach (string line in lines)
		{
			if (line.StartsWith("# branch.head ", StringComparison.Ordinal))
			{
				branch = line[14..];
				continue;
			}

			if (line.StartsWith("# branch.ab ", StringComparison.Ordinal))
			{
				ParseAheadBehind(line, out ahead, out behind);
				continue;
			}

			if (line.StartsWith("1 ", StringComparison.Ordinal))
			{
				ParseTrackedChange(line, changes);
				continue;
			}

			if (line.StartsWith("2 ", StringComparison.Ordinal))
			{
				ParseRenamedChange(line, changes);
				continue;
			}

			if (line.StartsWith("u ", StringComparison.Ordinal))
			{
				ParseUnmergedChange(line, changes);
				continue;
			}

			if (line.StartsWith("? ", StringComparison.Ordinal))
			{
				string filePath = line[2..].Trim();
				changes.Add(new GitFileChange(filePath, GitChangeKind.Untracked, "??", false));
				continue;
			}

			if (line.StartsWith("! ", StringComparison.Ordinal))
			{
				string filePath = line[2..].Trim();
				changes.Add(new GitFileChange(filePath, GitChangeKind.Ignored, "!!", false));
			}
		}

		return new RepositoryStatus(repositoryPath, branch, ahead, behind, changes);
	}

	private static void ParseAheadBehind(string line, out int ahead, out int behind)
	{
		ahead = 0;
		behind = 0;

		string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length < 4)
		{
			return;
		}

		_ = int.TryParse(tokens[2].TrimStart('+'), out ahead);
		_ = int.TryParse(tokens[3].TrimStart('-'), out behind);
	}

	private static void ParseTrackedChange(string line, List<GitFileChange> changes)
	{
		string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length < 9)
		{
			return;
		}

		string statusCode = tokens[1];
		string path = tokens[8];
		changes.Add(new GitFileChange(path, MapKind(statusCode), statusCode, IsStaged(statusCode)));
	}

	private static void ParseRenamedChange(string line, List<GitFileChange> changes)
	{
		string[] tabSeparated = line.Split('\t');
		if (tabSeparated.Length < 2)
		{
			return;
		}

		string[] tokens = tabSeparated[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length < 9)
		{
			return;
		}

		string statusCode = tokens[1];
		string newPath = tabSeparated[1];
		changes.Add(new GitFileChange(newPath, GitChangeKind.Renamed, statusCode, IsStaged(statusCode)));
	}

	private static void ParseUnmergedChange(string line, List<GitFileChange> changes)
	{
		string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (tokens.Length < 11)
		{
			return;
		}

		string statusCode = tokens[1];
		string path = tokens[10];
		changes.Add(new GitFileChange(path, GitChangeKind.Conflicted, statusCode, false));
	}

	private static GitChangeKind MapKind(string statusCode)
	{
		if (string.IsNullOrWhiteSpace(statusCode))
		{
			return GitChangeKind.Unknown;
		}

		if (statusCode.Contains('D', StringComparison.Ordinal)) return GitChangeKind.Deleted;
		if (statusCode.Contains('R', StringComparison.Ordinal)) return GitChangeKind.Renamed;
		if (statusCode.Contains('C', StringComparison.Ordinal)) return GitChangeKind.Copied;
		if (statusCode.Contains('A', StringComparison.Ordinal)) return GitChangeKind.Added;
		if (statusCode.Contains('U', StringComparison.Ordinal)) return GitChangeKind.Conflicted;
		if (statusCode.Contains('M', StringComparison.Ordinal) || statusCode.Contains('T', StringComparison.Ordinal)) return GitChangeKind.Modified;

		return GitChangeKind.Unknown;
	}

	private static bool IsStaged(string statusCode)
	{
		return !string.IsNullOrWhiteSpace(statusCode)
			&& statusCode[0] != '.'
			&& statusCode[0] != '?'
			&& statusCode[0] != '!';
	}

	private static async Task<string> RunGitAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
	{
		ProcessStartInfo startInfo = new()
		{
			FileName = "git",
			Arguments = arguments,
			WorkingDirectory = workingDirectory,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using Process process = new() { StartInfo = startInfo };
		process.Start();

		string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
		string stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

		await process.WaitForExitAsync(cancellationToken);

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"git {arguments} failed: {stderr}");
		}

		return stdout;
	}
}
