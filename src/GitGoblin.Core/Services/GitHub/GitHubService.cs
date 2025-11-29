using Octokit;

namespace GitGoblin.Core.Services.GitHub;

public class GitHubService : IGitService
{
    private GitHubClient? _client;
    
    public bool IsAuthenticated => _client != null;
    public Models.SourceType SourceType => Models.SourceType.GitHub;

    public async Task AuthenticateAsync(string token, string? serverUrl = null, CancellationToken cancellationToken = default)
    {
        var productHeader = new ProductHeaderValue("GitGoblin");
        
        if (!string.IsNullOrEmpty(serverUrl))
        {
            var enterpriseUri = new Uri(serverUrl);
            _client = new GitHubClient(productHeader, enterpriseUri);
        }
        else
        {
            _client = new GitHubClient(productHeader);
        }
        
        _client.Credentials = new Credentials(token);
        
        // Validate credentials by getting current user
        await _client.User.Current();
    }

    public async Task<Models.Account?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var user = await _client!.User.Current();
        return new Models.Account
        {
            Id = user.Id.ToString(),
            Name = user.Name ?? user.Login,
            Login = user.Login,
            AvatarUrl = user.AvatarUrl,
            Email = user.Email ?? string.Empty,
            Source = Models.SourceType.GitHub
        };
    }

    public async Task<IEnumerable<Models.Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var repos = await _client!.Repository.GetAllForCurrent();
        return repos.Select(MapRepository);
    }

    public async Task<Models.Repository?> GetRepositoryAsync(string owner, string name, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        try
        {
            var repo = await _client!.Repository.Get(owner, name);
            return MapRepository(repo);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Models.Issue>> GetIssuesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var request = new RepositoryIssueRequest
        {
            State = ItemStateFilter.All
        };
        
        var issues = await _client!.Issue.GetAllForRepository(owner, repo, request);
        return issues.Where(i => i.PullRequest == null).Select(i => MapIssue(i, $"{owner}/{repo}"));
    }

    public async Task<Models.Issue?> GetIssueAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        try
        {
            var issue = await _client!.Issue.Get(owner, repo, number);
            return MapIssue(issue, $"{owner}/{repo}");
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Models.Commit>> GetCommitsAsync(string owner, string repo, string? branch = null, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var request = new CommitRequest();
        if (!string.IsNullOrEmpty(branch))
        {
            request.Sha = branch;
        }
        
        var commits = await _client!.Repository.Commit.GetAll(owner, repo, request);
        return commits.Select(c => MapCommit(c, $"{owner}/{repo}"));
    }

    public async Task<Models.Commit?> GetCommitAsync(string owner, string repo, string sha, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        try
        {
            var commit = await _client!.Repository.Commit.Get(owner, repo, sha);
            return MapCommitDetailed(commit, $"{owner}/{repo}");
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Models.PullRequest>> GetPullRequestsAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var prs = await _client!.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest
        {
            State = ItemStateFilter.All
        });
        return prs.Select(pr => MapPullRequest(pr, $"{owner}/{repo}"));
    }

    public async Task<Models.PullRequest?> GetPullRequestAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        try
        {
            var pr = await _client!.PullRequest.Get(owner, repo, number);
            return MapPullRequestDetailed(pr, $"{owner}/{repo}");
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Models.PullRequestReview>> GetPullRequestReviewsAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var reviews = await _client!.PullRequest.Review.GetAll(owner, repo, number);
        return reviews.Select(MapPullRequestReview);
    }

    public async Task<IEnumerable<Models.PullRequestFile>> GetPullRequestFilesAsync(string owner, string repo, int number, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var files = await _client!.PullRequest.Files(owner, repo, number);
        return files.Select(MapPullRequestFile);
    }

    public async Task<Models.PullRequest> CreatePullRequestAsync(string owner, string repo, Models.CreatePullRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var newPr = new NewPullRequest(request.Title, request.HeadBranch, request.BaseBranch)
        {
            Body = request.Body,
            Draft = request.IsDraft
        };
        
        var pr = await _client!.PullRequest.Create(owner, repo, newPr);
        return MapPullRequestDetailed(pr, $"{owner}/{repo}");
    }

    public async Task<Models.PullRequestReview> SubmitPullRequestReviewAsync(string owner, string repo, int number, string body, string reviewEvent, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var review = new PullRequestReviewCreate
        {
            Body = body,
            Event = reviewEvent switch
            {
                "APPROVE" => Octokit.PullRequestReviewEvent.Approve,
                "REQUEST_CHANGES" => Octokit.PullRequestReviewEvent.RequestChanges,
                _ => Octokit.PullRequestReviewEvent.Comment
            }
        };
        
        var result = await _client!.PullRequest.Review.Create(owner, repo, number, review);
        return MapPullRequestReview(result);
    }

    public async Task<IEnumerable<Models.Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var notifications = await _client!.Activity.Notifications.GetAllForCurrent();
        return notifications.Select(MapNotification);
    }

    public async Task MarkNotificationAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        await _client!.Activity.Notifications.MarkAsRead(int.Parse(notificationId));
    }

    public async Task<IEnumerable<string>> GetBranchesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        
        var branches = await _client!.Repository.Branch.GetAll(owner, repo);
        return branches.Select(b => b.Name);
    }

    private void EnsureAuthenticated()
    {
        if (_client == null)
            throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
    }

    private static Models.Repository MapRepository(Octokit.Repository repo) => new()
    {
        Id = repo.Id,
        Name = repo.Name,
        FullName = repo.FullName,
        Description = repo.Description ?? string.Empty,
        Url = repo.Url,
        HtmlUrl = repo.HtmlUrl,
        CloneUrl = repo.CloneUrl,
        Language = repo.Language ?? string.Empty,
        StargazersCount = repo.StargazersCount,
        ForksCount = repo.ForksCount,
        OpenIssuesCount = repo.OpenIssuesCount,
        IsPrivate = repo.Private,
        IsFork = repo.Fork,
        CreatedAt = repo.CreatedAt,
        UpdatedAt = repo.UpdatedAt,
        DefaultBranch = repo.DefaultBranch,
        OwnerLogin = repo.Owner?.Login ?? string.Empty,
        OwnerAvatarUrl = repo.Owner?.AvatarUrl ?? string.Empty,
        Source = Models.SourceType.GitHub
    };

    private static Models.Issue MapIssue(Octokit.Issue issue, string repoFullName) => new()
    {
        Id = issue.Id,
        Number = issue.Number,
        Title = issue.Title,
        Body = issue.Body ?? string.Empty,
        State = issue.State.StringValue,
        HtmlUrl = issue.HtmlUrl,
        AuthorLogin = issue.User?.Login ?? string.Empty,
        AuthorAvatarUrl = issue.User?.AvatarUrl ?? string.Empty,
        Labels = issue.Labels?.Select(l => l.Name).ToList() ?? [],
        Assignee = issue.Assignee?.Login ?? string.Empty,
        CommentsCount = issue.Comments,
        CreatedAt = issue.CreatedAt,
        UpdatedAt = issue.UpdatedAt ?? issue.CreatedAt,
        ClosedAt = issue.ClosedAt,
        RepositoryFullName = repoFullName,
        Source = Models.SourceType.GitHub
    };

    private static Models.Commit MapCommit(GitHubCommit commit, string repoFullName) => new()
    {
        Sha = commit.Sha,
        Message = commit.Commit?.Message ?? string.Empty,
        AuthorName = commit.Commit?.Author?.Name ?? string.Empty,
        AuthorEmail = commit.Commit?.Author?.Email ?? string.Empty,
        AuthorAvatarUrl = commit.Author?.AvatarUrl ?? string.Empty,
        CommitDate = commit.Commit?.Author?.Date ?? DateTimeOffset.MinValue,
        HtmlUrl = commit.HtmlUrl,
        RepositoryFullName = repoFullName,
        Source = Models.SourceType.GitHub
    };

    private static Models.Commit MapCommitDetailed(GitHubCommit commit, string repoFullName)
    {
        var mapped = MapCommit(commit, repoFullName);
        mapped.AdditionsCount = commit.Stats?.Additions ?? 0;
        mapped.DeletionsCount = commit.Stats?.Deletions ?? 0;
        mapped.ChangedFilesCount = commit.Files?.Count ?? 0;
        return mapped;
    }

    private static Models.PullRequest MapPullRequest(Octokit.PullRequest pr, string repoFullName) => new()
    {
        Id = pr.Id,
        Number = pr.Number,
        Title = pr.Title,
        Body = pr.Body ?? string.Empty,
        State = pr.State.StringValue,
        HtmlUrl = pr.HtmlUrl,
        AuthorLogin = pr.User?.Login ?? string.Empty,
        AuthorAvatarUrl = pr.User?.AvatarUrl ?? string.Empty,
        HeadRef = pr.Head?.Ref ?? string.Empty,
        BaseRef = pr.Base?.Ref ?? string.Empty,
        IsMerged = pr.Merged,
        CreatedAt = pr.CreatedAt,
        UpdatedAt = pr.UpdatedAt,
        MergedAt = pr.MergedAt,
        ClosedAt = pr.ClosedAt,
        RepositoryFullName = repoFullName,
        Source = Models.SourceType.GitHub,
        Labels = pr.Labels?.Select(l => l.Name).ToList() ?? []
    };

    private static Models.PullRequest MapPullRequestDetailed(Octokit.PullRequest pr, string repoFullName)
    {
        var mapped = MapPullRequest(pr, repoFullName);
        mapped.CommitsCount = pr.Commits;
        mapped.ChangedFilesCount = pr.ChangedFiles;
        mapped.AdditionsCount = pr.Additions;
        mapped.DeletionsCount = pr.Deletions;
        mapped.ReviewCommentsCount = pr.Comments;
        mapped.IsMergeable = pr.Mergeable ?? false;
        return mapped;
    }

    private static Models.PullRequestReview MapPullRequestReview(Octokit.PullRequestReview review) => new()
    {
        Id = review.Id,
        Body = review.Body ?? string.Empty,
        State = review.State.StringValue,
        AuthorLogin = review.User?.Login ?? string.Empty,
        AuthorAvatarUrl = review.User?.AvatarUrl ?? string.Empty,
        SubmittedAt = review.SubmittedAt,
        HtmlUrl = review.HtmlUrl
    };

    private static Models.PullRequestFile MapPullRequestFile(Octokit.PullRequestFile file) => new()
    {
        FileName = file.FileName,
        Status = file.Status,
        Additions = file.Additions,
        Deletions = file.Deletions,
        Changes = file.Changes,
        Patch = file.Patch ?? string.Empty,
        BlobUrl = file.BlobUrl
    };

    private static Models.Notification MapNotification(Octokit.Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Subject?.Title ?? string.Empty,
        Type = notification.Subject?.Type ?? string.Empty,
        Reason = notification.Reason,
        IsUnread = notification.Unread,
        SubjectUrl = notification.Subject?.Url ?? string.Empty,
        RepositoryFullName = notification.Repository?.FullName ?? string.Empty,
        UpdatedAt = DateTimeOffset.TryParse(notification.UpdatedAt, out var dt) ? dt : DateTimeOffset.MinValue,
        Source = Models.SourceType.GitHub
    };
}
