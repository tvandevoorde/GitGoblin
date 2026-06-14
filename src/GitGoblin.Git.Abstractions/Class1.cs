using GitGoblin.Core;

namespace GitGoblin.Git.Abstractions;

public interface IGitRepositoryService
{
	Task<RepositoryStatus> GetStatusAsync(string repositoryPath, CancellationToken cancellationToken = default);
}
