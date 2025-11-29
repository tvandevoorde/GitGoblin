using GitGoblin.Core.ViewModels;

namespace GitGoblin.App;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	private async void OnRepositoriesTapped(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//RepositoriesPage");
	}

	private async void OnNotificationsTapped(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//NotificationsPage");
	}

	private async void OnPullRequestsTapped(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//RepositoriesPage");
	}

	private async void OnSettingsTapped(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//SettingsPage");
	}
}
