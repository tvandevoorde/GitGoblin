using GitGoblin.Core.Models;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitGoblin.Core.Services.AzureDevOps;

public class AzureDevOpsService : IGitService
{
    private VssConnection? _connection;
    private string _serverUrl = string.Empty;
    private string _currentUserName = string.Empty;
    private string _currentUserId = string.Empty;

    public bool IsAuthenticated => _connection != null;
    public SourceType SourceType => SourceType.AzureDevOps;

    public async Task AuthenticateAsync(string token, string? serverUrl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(serverUrl))
            throw new ArgumentException("Server URL is required for Azure DevOps", nameof(serverUrl));

        _serverUrl = serverUrl;
        var credentials = new VssBasicCredential(string.Empty, token);
        _connection = new VssConnection(new Uri(serverUrl), credentials);

        // Validate connection
        var client = await _connection.GetClientAsync<ProjectHttpClient>(cancellationToken);
        await client.GetProjects();

        // Get current user info
        var connectionData = _connection.AuthorizedIdentity;
        _currentUserName = connectionData.DisplayName;
        _currentUserId = connectionData.Id.ToString();
    }

    public Task<Account?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        return Task.FromResult<Account?>(new Account
        {
            Id = _currentUserId,
            Name = _currentUserName,
            Login = _currentUserName,
            ServerUrl = _serverUrl,
            Source = SourceType.AzureDevOps
        });
    }

    public async Task<IEnumerable<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        var repos = await gitClient.GetRepositoriesAsync(cancellationToken: cancellationToken);
        
        return repos.Select(MapRepository);
    }

    public async Task<Repository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        try
        {
            var repo = await gitClient.GetRepositoryAsync(owner, name, cancellationToken: cancellationToken);
            return MapRepository(repo);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var witClient = await _connection!.GetClientAsync<WorkItemTrackingHttpClient>(cancellationToken);
        
        var wiql = new Wiql
        {
            Query = $"SELECT [System.Id], [System.Title], [System.State], [System.WorkItemType] " +
                    $"FROM WorkItems " +
                    $"WHERE [System.TeamProject] = '{owner}' " +
                    $"AND [System.WorkItemType] IN ('Bug', 'Task', 'User Story', 'Issue') " +
                    $"ORDER BY [System.CreatedDate] DESC"
        };
        
        var result = await witClient.QueryByWiqlAsync(wiql, cancellationToken: cancellationToken);
        
        if (result.WorkItems == null || !result.WorkItems.Any())
            return Enumerable.Empty<Issue>();

        var ids = result.WorkItems.Select(wi => wi.Id).ToArray();
        var workItems = await witClient.GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
        
        return workItems.Select(wi => MapWorkItemToIssue(wi, $"{owner}/{repo}"));
    }

    public async Task<Issue?> GetIssueAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var witClient = await _connection!.GetClientAsync<WorkItemTrackingHttpClient>(cancellationToken);
        
        try
        {
            var workItem = await witClient.GetWorkItemAsync(number, cancellationToken: cancellationToken);
            return MapWorkItemToIssue(workItem, $"{owner}/{repo}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<Commit>> GetCommitsAsync(string owner, string repo, string? branch = null, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var searchCriteria = new GitQueryCommitsCriteria();
        if (!string.IsNullOrEmpty(branch))
        {
            searchCriteria.ItemVersion = new GitVersionDescriptor
            {
                Version = branch,
                VersionType = GitVersionType.Branch
            };
        }
        
        var commits = await gitClient.GetCommitsAsync(owner, repo, searchCriteria, cancellationToken: cancellationToken);
        return commits.Select(c => MapCommit(c, $"{owner}/{repo}"));
    }

    public async Task<Commit?> GetCommitAsync(string owner, string repo, string sha, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        try
        {
            var commit = await gitClient.GetCommitAsync(sha, owner, repo, cancellationToken: cancellationToken);
            return MapCommitDetailed(commit, $"{owner}/{repo}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<PullRequest>> GetPullRequestsAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var searchCriteria = new GitPullRequestSearchCriteria
        {
            Status = PullRequestStatus.All
        };
        
        var prs = await gitClient.GetPullRequestsAsync(owner, repo, searchCriteria, cancellationToken: cancellationToken);
        return prs.Select(pr => MapPullRequest(pr, $"{owner}/{repo}"));
    }

    public async Task<PullRequest?> GetPullRequestAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        try
        {
            var pr = await gitClient.GetPullRequestAsync(owner, repo, number, cancellationToken: cancellationToken);
            return MapPullRequestDetailed(pr, $"{owner}/{repo}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<PullRequestReview>> GetPullRequestReviewsAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var threads = await gitClient.GetThreadsAsync(owner, repo, number, cancellationToken: cancellationToken);
        
        return threads.Select(t => new PullRequestReview
        {
            Id = t.Id,
            Body = t.Comments?.FirstOrDefault()?.Content ?? string.Empty,
            State = t.Status.ToString() ?? "Unknown",
            AuthorLogin = t.Comments?.FirstOrDefault()?.Author?.DisplayName ?? string.Empty,
            SubmittedAt = t.Comments?.FirstOrDefault()?.PublishedDate ?? DateTimeOffset.MinValue
        });
    }

    public async Task<IEnumerable<PullRequestFile>> GetPullRequestFilesAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var iterations = await gitClient.GetPullRequestIterationsAsync(owner, repo, number, cancellationToken: cancellationToken);
        
        if (iterations == null || iterations.Count == 0)
            return Enumerable.Empty<PullRequestFile>();

        var lastIteration = iterations.Last();
        var changes = await gitClient.GetPullRequestIterationChangesAsync(owner, repo, number, lastIteration.Id!.Value, cancellationToken: cancellationToken);
        
        return changes.ChangeEntries?.Select(c => new PullRequestFile
        {
            FileName = c.Item?.Path ?? string.Empty,
            Status = c.ChangeType.ToString() ?? "Unknown"
        }) ?? Enumerable.Empty<PullRequestFile>();
    }

    public async Task<PullRequest> CreatePullRequestAsync(string owner, string repo, CreatePullRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var newPr = new GitPullRequest
        {
            Title = request.Title,
            Description = request.Body,
            SourceRefName = $"refs/heads/{request.HeadBranch}",
            TargetRefName = $"refs/heads/{request.BaseBranch}",
            IsDraft = request.IsDraft
        };
        
        var pr = await gitClient.CreatePullRequestAsync(newPr, owner, repo, cancellationToken: cancellationToken);
        return MapPullRequestDetailed(pr, $"{owner}/{repo}");
    }

    public async Task<PullRequestReview> SubmitPullRequestReviewAsync(string owner, string repo, int number, string body, string reviewEvent, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        
        var thread = new GitPullRequestCommentThread
        {
            Comments = new List<Microsoft.TeamFoundation.SourceControl.WebApi.Comment>
            {
                new() { Content = body }
            },
            Status = reviewEvent switch
            {
                "APPROVE" => CommentThreadStatus.Fixed,
                "REQUEST_CHANGES" => CommentThreadStatus.Active,
                _ => CommentThreadStatus.Unknown
            }
        };
        
        var result = await gitClient.CreateThreadAsync(thread, owner, repo, number, cancellationToken: cancellationToken);
        
        return new PullRequestReview
        {
            Id = result.Id,
            Body = body,
            State = reviewEvent,
            SubmittedAt = DateTimeOffset.UtcNow
        };
    }

    public Task<IEnumerable<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        // Azure DevOps doesn't have a direct notifications API like GitHub
        // This would require implementing through the Activity Feed or Alerts API
        return Task.FromResult(Enumerable.Empty<Notification>());
    }

    public Task MarkNotificationAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        // Not implemented for Azure DevOps
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<string>> GetBranchesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var gitClient = await _connection!.GetClientAsync<GitHttpClient>(cancellationToken);
        var branches = await gitClient.GetBranchesAsync(owner, repo, cancellationToken: cancellationToken);
        
        return branches.Select(b => b.Name);
    }

    private void EnsureAuthenticated()
    {
        if (_connection == null)
            throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
    }

    private static Models.Repository MapRepository(GitRepository repo) => new()
    {
        Id = BitConverter.ToInt64(repo.Id.ToByteArray(), 0),
        Name = repo.Name,
        FullName = $"{repo.ProjectReference?.Name}/{repo.Name}",
        Description = string.Empty,
        Url = repo.Url,
        HtmlUrl = repo.WebUrl,
        CloneUrl = repo.RemoteUrl,
        DefaultBranch = repo.DefaultBranch?.Replace("refs/heads/", "") ?? "main",
        IsPrivate = true, // Azure DevOps repos are private by default
        OwnerLogin = repo.ProjectReference?.Name ?? string.Empty,
        Source = SourceType.AzureDevOps
    };

    private static Models.Issue MapWorkItemToIssue(WorkItem workItem, string repoFullName) => new()
    {
        Id = workItem.Id ?? 0,
        Number = workItem.Id ?? 0,
        Title = workItem.Fields?.GetValueOrDefault("System.Title")?.ToString() ?? string.Empty,
        Body = workItem.Fields?.GetValueOrDefault("System.Description")?.ToString() ?? string.Empty,
        State = workItem.Fields?.GetValueOrDefault("System.State")?.ToString() ?? string.Empty,
        AuthorLogin = workItem.Fields?.GetValueOrDefault("System.CreatedBy")?.ToString() ?? string.Empty,
        CreatedAt = workItem.Fields?.GetValueOrDefault("System.CreatedDate") is DateTime created 
            ? new DateTimeOffset(created) 
            : DateTimeOffset.MinValue,
        UpdatedAt = workItem.Fields?.GetValueOrDefault("System.ChangedDate") is DateTime changed 
            ? new DateTimeOffset(changed) 
            : DateTimeOffset.MinValue,
        RepositoryFullName = repoFullName,
        Source = SourceType.AzureDevOps
    };

    private static Models.Commit MapCommit(GitCommitRef commit, string repoFullName) => new()
    {
        Sha = commit.CommitId,
        Message = commit.Comment ?? string.Empty,
        AuthorName = commit.Author?.Name ?? string.Empty,
        AuthorEmail = commit.Author?.Email ?? string.Empty,
        CommitDate = commit.Author?.Date ?? DateTimeOffset.MinValue,
        HtmlUrl = commit.RemoteUrl,
        RepositoryFullName = repoFullName,
        Source = SourceType.AzureDevOps
    };

    private static Models.Commit MapCommitDetailed(GitCommit commit, string repoFullName) => new()
    {
        Sha = commit.CommitId,
        Message = commit.Comment ?? string.Empty,
        AuthorName = commit.Author?.Name ?? string.Empty,
        AuthorEmail = commit.Author?.Email ?? string.Empty,
        CommitDate = commit.Author?.Date ?? DateTimeOffset.MinValue,
        HtmlUrl = commit.RemoteUrl,
        ChangedFilesCount = commit.ChangeCounts?.Values.Sum() ?? 0,
        RepositoryFullName = repoFullName,
        Source = SourceType.AzureDevOps
    };

    private static Models.PullRequest MapPullRequest(GitPullRequest pr, string repoFullName) => new()
    {
        Id = pr.PullRequestId,
        Number = pr.PullRequestId,
        Title = pr.Title ?? string.Empty,
        Body = pr.Description ?? string.Empty,
        State = pr.Status.ToString() ?? "Unknown",
        AuthorLogin = pr.CreatedBy?.DisplayName ?? string.Empty,
        HeadRef = pr.SourceRefName?.Replace("refs/heads/", "") ?? string.Empty,
        BaseRef = pr.TargetRefName?.Replace("refs/heads/", "") ?? string.Empty,
        IsMerged = pr.Status == PullRequestStatus.Completed,
        CreatedAt = pr.CreationDate,
        UpdatedAt = pr.CreationDate,
        ClosedAt = pr.ClosedDate,
        RepositoryFullName = repoFullName,
        Source = SourceType.AzureDevOps
    };

    private static Models.PullRequest MapPullRequestDetailed(GitPullRequest pr, string repoFullName)
    {
        var mapped = MapPullRequest(pr, repoFullName);
        mapped.IsMergeable = pr.MergeStatus == PullRequestAsyncStatus.Succeeded;
        mapped.ReviewCommentsCount = pr.Reviewers?.Length ?? 0;
        return mapped;
    }
}
