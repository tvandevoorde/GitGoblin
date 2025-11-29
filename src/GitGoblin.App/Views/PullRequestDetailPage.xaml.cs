using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
[QueryProperty(nameof(Number), "number")]
public partial class PullRequestDetailPage : ContentPage
{
    private readonly PullRequestDetailViewModel _viewModel;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public int Number { get; set; }

    public PullRequestDetailPage(PullRequestDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize(Owner, Repo, Number);
        await _viewModel.LoadPullRequestCommand.ExecuteAsync(null);
    }

    private void OnReviewActionChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton radioButton && radioButton.Value is string action)
        {
            _viewModel.SelectedReviewAction = action;
        }
    }
}
