using GitGoblin.Core.Models;
using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
public partial class PullRequestsPage : ContentPage
{
    private readonly PullRequestsViewModel _viewModel;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;

    public PullRequestsPage(PullRequestsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize(Owner, Repo);
        await _viewModel.LoadPullRequestsCommand.ExecuteAsync(null);
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string filter)
        {
            _viewModel.FilterState = filter;
        }
    }

    private async void OnPullRequestSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PullRequest pr)
        {
            await Shell.Current.GoToAsync($"PullRequestDetailPage?owner={Owner}&repo={Repo}&number={pr.Number}");
            
            // Clear selection
            ((CollectionView)sender!).SelectedItem = null;
        }
    }
}
