using GitGoblin.Core.Models;
using GitGoblin.Core.ViewModels;

namespace GitGoblin.App.Views;

public partial class RepositoriesPage : ContentPage
{
    private readonly RepositoriesViewModel _viewModel;

    public RepositoriesPage(RepositoriesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRepositoriesCommand.ExecuteAsync(null);
    }

    private async void OnRepositorySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Repository repo)
        {
            var parts = repo.FullName.Split('/');
            if (parts.Length == 2)
            {
                await Shell.Current.GoToAsync($"RepositoryDetailPage?owner={parts[0]}&repo={parts[1]}");
            }
            
            // Clear selection
            ((CollectionView)sender!).SelectedItem = null;
        }
    }
}
