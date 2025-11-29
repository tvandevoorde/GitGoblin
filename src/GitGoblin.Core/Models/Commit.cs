namespace GitGoblin.Core.Models;

public class Commit
{
    public string Sha { get; set; } = string.Empty;
    public string ShortSha => Sha.Length > 7 ? Sha[..7] : Sha;
    public string Message { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset CommitDate { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public int AdditionsCount { get; set; }
    public int DeletionsCount { get; set; }
    public int ChangedFilesCount { get; set; }
    public string RepositoryFullName { get; set; } = string.Empty;
    public SourceType Source { get; set; }
}
