using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitGoblin.Core.Models;
using GitGoblin.Core.Services;
using System.Collections.ObjectModel;

namespace GitGoblin.Core.ViewModels;

public partial class PullRequestDetailViewModel : ObservableObject
{
    private readonly IGitService _gitService;
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    private int _pullRequestNumber;
    
    [ObservableProperty]
    private PullRequest? _pullRequest;
    
    [ObservableProperty]
    private ObservableCollection<PullRequestReview> _reviews = [];
    
    [ObservableProperty]
    private ObservableCollection<PullRequestFile> _files = [];
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private string _reviewComment = string.Empty;
    
    [ObservableProperty]
    private string _selectedReviewAction = "COMMENT";

    public PullRequestDetailViewModel(IGitService gitService)
    {
        _gitService = gitService;
    }

    public void Initialize(string owner, string repo, int number)
    {
        _owner = owner;
        _repo = repo;
        _pullRequestNumber = number;
    }

    [RelayCommand]
    private async Task LoadPullRequestAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo) || _pullRequestNumber == 0)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            PullRequest = await _gitService.GetPullRequestAsync(_owner, _repo, _pullRequestNumber);
            
            var reviewsTask = _gitService.GetPullRequestReviewsAsync(_owner, _repo, _pullRequestNumber);
            var filesTask = _gitService.GetPullRequestFilesAsync(_owner, _repo, _pullRequestNumber);
            
            await Task.WhenAll(reviewsTask, filesTask);
            
            Reviews = new ObservableCollection<PullRequestReview>(await reviewsTask);
            Files = new ObservableCollection<PullRequestFile>(await filesTask);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load pull request: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (string.IsNullOrEmpty(_owner) || string.IsNullOrEmpty(_repo) || _pullRequestNumber == 0)
            return;

        if (string.IsNullOrWhiteSpace(ReviewComment))
        {
            ErrorMessage = "Please enter a review comment";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var review = await _gitService.SubmitPullRequestReviewAsync(_owner, _repo, _pullRequestNumber, 
                ReviewComment, SelectedReviewAction);
            
            Reviews.Add(review);
            ReviewComment = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to submit review: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPullRequestAsync();
    }
}
