using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class IssuesViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Issue> _issues = [];
    
    [ObservableProperty]
    private Issue? _selectedIssue;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _filterState = "all";

    public IssuesViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    public void Initialize(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;
    }

    [RelayCommand]
    private async Task LoadIssuesAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var issues = await _gitService.GetIssuesAsync(_owner, _repo);
            
            if (FilterState != "all")
            {
                issues = issues.Where(i => i.State.Equals(FilterState, StringComparison.OrdinalIgnoreCase));
            }
            
            Issues = new ObservableCollection<Issue>(issues);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load issues: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadIssuesAsync();
    }

    partial void OnFilterStateChanged(string value)
    {
        _ = LoadIssuesAsync();
    }
}
