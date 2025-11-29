namespace GitGoblin.Core.Models;

public class PullRequest
{
    public long Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty;
    public string HeadRef { get; set; } = string.Empty;
    public string BaseRef { get; set; } = string.Empty;
    public bool IsMerged { get; set; }
    public bool IsMergeable { get; set; }
    public int CommitsCount { get; set; }
    public int ChangedFilesCount { get; set; }
    public int AdditionsCount { get; set; }
    public int DeletionsCount { get; set; }
    public int ReviewCommentsCount { get; set; }
    public List<string> Labels { get; set; } = [];
    public List<string> Reviewers { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? MergedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string RepositoryFullName { get; set; } = string.Empty;
    public SourceType Source { get; set; }
}
