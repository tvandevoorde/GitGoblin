using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class PullRequestsViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<PullRequest> _pullRequests = [];
    
    [ObservableProperty]
    private PullRequest? _selectedPullRequest;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _filterState = "all";

    public PullRequestsViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    public void Initialize(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;
    }

    [RelayCommand]
    private async Task LoadPullRequestsAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var prs = await _gitService.GetPullRequestsAsync(_owner, _repo);
            
            if (FilterState != "all")
            {
                prs = prs.Where(p => p.State.Equals(FilterState, StringComparison.OrdinalIgnoreCase));
            }
            
            PullRequests = new ObservableCollection<PullRequest>(prs);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load pull requests: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPullRequestsAsync();
    }

    partial void OnFilterStateChanged(string value)
    {
        _ = LoadPullRequestsAsync();
    }
}
