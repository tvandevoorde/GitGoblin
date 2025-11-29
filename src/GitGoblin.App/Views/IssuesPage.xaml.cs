using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

[QueryProperty(nameof(Owner), "owner")]
[QueryProperty(nameof(Repo), "repo")]
public partial class IssuesPage : ContentPage
{
    private readonly IssuesViewModel _viewModel;
    
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;

    public IssuesPage(IssuesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Initialize(Owner, Repo);
        await _viewModel.LoadIssuesCommand.ExecuteAsync(null);
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string filter)
        {
            _viewModel.FilterState = filter;
        }
    }
}
