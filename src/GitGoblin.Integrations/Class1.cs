namespace GitGoblin.Integrations;

public interface IAiAssistantService
{
	Task<string> GenerateCommitMessageAsync(string repositoryPath, CancellationToken cancellationToken = default);
}

public interface IExternalEditorBridge
{
	Task OpenRepositoryAsync(string repositoryPath, CancellationToken cancellationToken = default);
}

public interface IAzureDevOpsServerService
{
	Task<bool> IsReachableAsync(Uri serverUri, CancellationToken cancellationToken = default);
}
