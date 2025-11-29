namespace GitGoblin.Core.Models;

public class Repository
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsFork { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string DefaultBranch { get; set; } = string.Empty;
    public string OwnerLogin { get; set; } = string.Empty;
    public string OwnerAvatarUrl { get; set; } = string.Empty;
    public SourceType Source { get; set; }
}
