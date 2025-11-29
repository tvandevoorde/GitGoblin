using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class CommitsViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Commit> _commits = [];
    
    [ObservableProperty]
    private Commit? _selectedCommit;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _selectedBranch = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<string> _branches = [];

    public CommitsViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    public void Initialize(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;
    }

    [RelayCommand]
    private async Task LoadCommitsAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var commits = await _gitService.GetCommitsAsync(_owner, _repo, 
                string.IsNullOrEmpty(SelectedBranch) ? null : SelectedBranch);
            Commits = new ObservableCollection<Commit>(commits);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load commits: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadBranchesAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        try
        {
            var branches = await _gitService.GetBranchesAsync(_owner, _repo);
            Branches = new ObservableCollection<string>(branches);
            
            if (Branches.Count > 0 && string.IsNullOrEmpty(SelectedBranch))
            {
                SelectedBranch = Branches[0];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load branches: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCommitsAsync();
    }

    partial void OnSelectedBranchChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadCommitsAsync();
        }
    }
}
