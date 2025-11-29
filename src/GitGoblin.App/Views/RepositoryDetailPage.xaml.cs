using GitGoblin.Core.Services;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
public partial class RepositoryDetailPage : ContentPage
{
    private readonly IGitService _gitService;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public string RepositoryName => $"{Owner}/{Repo}";
    public string Description { get; set; } = string.Empty;

    public RepositoryDetailPage()
    {
        InitializeComponent();
        _gitService = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<GitGoblin.Core.Services.GitHub.GitHubService>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRepositoryAsync();
    }

    private async Task LoadRepositoryAsync()
    {
        if (string.IsNullOrEmpty(Owner) || string.IsNullOrEmpty(Repo))
            return;

        try
        {
            var repository = await _gitService.GetRepositoryAsync(Owner, Repo);
            if (repository != null)
            {
                Description = repository.Description;
                StarsLabel.Text = repository.StargazersCount.ToString();
                ForksLabel.Text = repository.ForksCount.ToString();
                IssuesLabel.Text = repository.OpenIssuesCount.ToString();
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(RepositoryName));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load repository: {ex.Message}", "OK");
        }
    }

    private async void OnIssuesTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"IssuesPage?owner={Owner}&repo={Repo}");
    }

    private async void OnCommitsTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"CommitsPage?owner={Owner}&repo={Repo}");
    }

    private async void OnPullRequestsTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"PullRequestsPage?owner={Owner}&repo={Repo}");
    }

    private async void OnCreatePRTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"CreatePullRequestPage?owner={Owner}&repo={Repo}");
    }

    private async void OnViewBranchesTapped(object? sender, EventArgs e)
    {
        try
        {
            var branches = await _gitService.GetBranchesAsync(Owner, Repo);
            var branchList = string.Join("\n", branches);
            await DisplayAlert("Branches", branchList, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load branches: {ex.Message}", "OK");
        }
    }
}
