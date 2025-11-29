using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;

namespace GitGoblin.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IGitService _gitHubService;
    private readonly IGitService _azureDevOpsService;
    
    [ObservableProperty]
    private Account? _currentAccount;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isGitHubConnected;
    
    [ObservableProperty]
    private bool _isAzureDevOpsConnected;

    public MainViewModel(IGitService gitHubService, IGitService azureDevOpsService)
    {
        _gitHubService = gitHubService;
        _azureDevOpsService = azureDevOpsService;
    }

    public IGitService GitHubService => _gitHubService;
    public IGitService AzureDevOpsService => _azureDevOpsService;

    [RelayCommand]
    private async Task ConnectToGitHubAsync(string token)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            await _gitHubService.AuthenticateAsync(token);
            CurrentAccount = await _gitHubService.GetCurrentUserAsync();
            IsGitHubConnected = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to connect to GitHub: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ConnectToAzureDevOpsAsync((string token, string serverUrl) credentials)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            await _azureDevOpsService.AuthenticateAsync(credentials.token, credentials.serverUrl);
            CurrentAccount = await _azureDevOpsService.GetCurrentUserAsync();
            IsAzureDevOpsConnected = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to connect to Azure DevOps: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
