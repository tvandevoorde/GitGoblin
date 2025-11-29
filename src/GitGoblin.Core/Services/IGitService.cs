using GitGoblin.Core.Models;

namespace GitGoblin.Core.Services;

public interface IGitService
{
    Task<IEnumerable<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default);
    Task<Repository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<Issue?> GetIssueAsync(string owner, string repo, int number, CancellationToken cancellationToken = default);
    Task<IEnumerable<Commit>> GetCommitsAsync(string owner, string repo, string? branch = null, CancellationToken cancellationToken = default);
    Task<Commit?> GetCommitAsync(string owner, string repo, string sha, CancellationToken cancellationToken = default);
    Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<PullRequest?> GetPullRequestAsync(string owner, string repo, int number, CancellationToken cancellationToken = default);
    Task<IEnumerable<PullRequestReview>> GetPullRequestReviewsAsync(string owner, string repo, int number, CancellationToken cancellationToken = default);
    Task<IEnumerable<PullRequestFile>> GetPullRequestFilesAsync(string owner, string repo, int number, CancellationToken cancellationToken = default);
    Task<PullRequest> CreatePullRequestAsync(string owner, string repo, CreatePullRequestRequest request, CancellationToken cancellationToken = default);
    Task<PullRequestReview> SubmitPullRequestReviewAsync(string owner, string repo, int number, string body, string reviewEvent, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default);
    Task MarkNotificationAsReadAsync(string notificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetBranchesAsync(string owner, string repo, CancellationToken cancellationToken = default);
    bool IsAuthenticated { get; }
    SourceType SourceType { get; }
    Task AuthenticateAsync(string token, string? serverUrl = null, CancellationToken cancellationToken = default);
    Task<Account?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
