namespace GitGoblin.Core.Models;

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsUnread { get; set; }
    public string SubjectUrl { get; set; } = string.Empty;
    public string RepositoryFullName { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public SourceType Source { get; set; }
}
