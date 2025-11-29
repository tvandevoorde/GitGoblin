using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class CreatePullRequestViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _body = string.Empty;
    
    [ObservableProperty]
    private string _headBranch = string.Empty;
    
    [ObservableProperty]
    private string _baseBranch = string.Empty;
    
    [ObservableProperty]
    private bool _isDraft;
    
    [ObservableProperty]
    private ObservableCollection<string> _branches = [];
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private PullRequest? _createdPullRequest;

    public CreatePullRequestViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    public void Initialize(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;
    }

    [RelayCommand]
    private async Task LoadBranchesAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        try
        {
            IsLoading = true;
            var branches = await _gitService.GetBranchesAsync(_owner, _repo);
            Branches = new ObservableCollection<string>(branches);
            
            if (Branches.Count > 0)
            {
                BaseBranch = Branches.FirstOrDefault(b => b == "main" || b == "master") ?? Branches[0];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load branches: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreatePullRequestAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo))
            return;

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Title is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(HeadBranch) || string.IsNullOrWhiteSpace(BaseBranch))
        {
            ErrorMessage = "Please select source and target branches";
            return;
        }

        if (HeadBranch == BaseBranch)
        {
            ErrorMessage = "Source and target branches must be different";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var request = new CreatePullRequestRequest
            {
                Title = Title,
                Body = Body,
                HeadBranch = HeadBranch,
                BaseBranch = BaseBranch,
                IsDraft = IsDraft
            };
            
            CreatedPullRequest = await _gitService.CreatePullRequestAsync(_owner, _repo, request);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create pull request: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
