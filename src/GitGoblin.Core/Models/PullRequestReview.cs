namespace GitGoblin.Core.Models;

public class PullRequestReview
{
    public long Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string AuthorAvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}

public class PullRequestFile
{
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int Changes { get; set; }
    public string Patch { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
}

public class CreatePullRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string HeadBranch { get; set; } = string.Empty;
    public string BaseBranch { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
}
