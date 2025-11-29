using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
public partial class CommitsPage : ContentPage
{
    private readonly CommitsViewModel _viewModel;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;

    public CommitsPage(CommitsViewModel viewModel)
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
        await _viewModel.LoadCommitsCommand.ExecuteAsync(null);
    }
}
