namespace GitGoblin.Core;

public enum GitChangeKind
{
	Unknown,
	Added,
	Modified,
	Deleted,
	Renamed,
	Copied,
	Conflicted,
	Untracked,
	Ignored
}

public sealed record GitFileChange(
	string Path,
	GitChangeKind Kind,
	string StatusCode,
	bool IsStaged);

public sealed record RepositoryStatus(
	string RepositoryPath,
	string CurrentBranch,
	int AheadBy,
	int BehindBy,
	IReadOnlyList<GitFileChange> Changes);
