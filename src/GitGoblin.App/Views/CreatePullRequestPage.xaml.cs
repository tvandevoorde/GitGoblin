using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
public partial class CreatePullRequestPage : ContentPage
{
    private readonly CreatePullRequestViewModel _viewModel;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;

    public CreatePullRequestPage(CreatePullRequestViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize(Owner, Repo);
        await _viewModel.LoadBranchesCommand.ExecuteAsync(null);
    }
}
