using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class RepositoriesViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    
    [ObservableProperty]
    private ObservableCollection<Repository> _repositories = [];
    
    [ObservableProperty]
    private Repository? _selectedRepository;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public RepositoriesViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    [RelayCommand]
    private async Task LoadRepositoriesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var repos = await _gitService.GetRepositoriesAsync();
            Repositories = new ObservableCollection<Repository>(repos);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load repositories: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadRepositoriesAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _ = LoadRepositoriesAsync();
        }
        else
        {
            var filtered = Repositories.Where(r => 
                r.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                (r.Description?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
            Repositories = new ObservableCollection<Repository>(filtered);
        }
    }
}
