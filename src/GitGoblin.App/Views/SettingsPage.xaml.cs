using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

public partial class SettingsPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public SettingsPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnConnectGitHubClicked(object? sender, EventArgs e)
    {
        var token = GitHubTokenEntry.Text;
        if (string.IsNullOrWhiteSpace(token))
        {
            await DisplayAlert("Error", "Please enter your GitHub Personal Access Token", "OK");
            return;
        }

        await _viewModel.ConnectToGitHubCommand.ExecuteAsync(token);
        
        if (_viewModel.IsGitHubConnected)
        {
            GitHubTokenEntry.Text = string.Empty;
            GitHubServerEntry.Text = string.Empty;
            await DisplayAlert("Success", "Connected to GitHub successfully!", "OK");
        }
    }

    private async void OnConnectAzureDevOpsClicked(object? sender, EventArgs e)
    {
        var serverUrl = AzureDevOpsServerEntry.Text;
        var token = AzureDevOpsTokenEntry.Text;
        
        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            await DisplayAlert("Error", "Please enter your Azure DevOps Server URL", "OK");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(token))
        {
            await DisplayAlert("Error", "Please enter your Personal Access Token", "OK");
            return;
        }

        await _viewModel.ConnectToAzureDevOpsCommand.ExecuteAsync((token, serverUrl));
        
        if (_viewModel.IsAzureDevOpsConnected)
        {
            AzureDevOpsTokenEntry.Text = string.Empty;
            AzureDevOpsServerEntry.Text = string.Empty;
            await DisplayAlert("Success", "Connected to Azure DevOps successfully!", "OK");
        }
    }

    private async void OnDisconnectGitHubClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Disconnect", "Are you sure you want to disconnect from GitHub?", "Yes", "No");
        if (confirm)
        {
            _viewModel.IsGitHubConnected = false;
            _viewModel.CurrentAccount = null;
        }
    }

    private async void OnDisconnectAzureDevOpsClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Disconnect", "Are you sure you want to disconnect from Azure DevOps?", "Yes", "No");
        if (confirm)
        {
            _viewModel.IsAzureDevOpsConnected = false;
            _viewModel.CurrentAccount = null;
        }
    }
}
