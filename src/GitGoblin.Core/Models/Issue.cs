namespace GitGoblin.Core.Models;

public class Issue
{
    public long Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = [];
    public string Assignee { get; set; } = string.Empty;
    public int CommentsCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string RepositoryFullName { get; set; } = string.Empty;
    public SourceType Source { get; set; }
}
