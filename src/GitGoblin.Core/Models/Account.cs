namespace GitGoblin.Core.Models;

public class Account
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public SourceType Source { get; set; }
    public bool IsDefault { get; set; }
}
