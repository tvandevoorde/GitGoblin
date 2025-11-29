using GitGoblin.Core.Services;
using GitGoblin.Core.Services.AzureDevOps;
using GitGoblin.Core.Services.GitHub;
using GitGoblin.Core.ViewModels;
using Microsoft.Extensions.Logging;

namespace GitGoblin.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddSingleton<GitHubService>();
		builder.Services.AddSingleton<AzureDevOpsService>();
		
		// Register ViewModels
		builder.Services.AddSingleton<MainViewModel>(sp => 
			new MainViewModel(sp.GetRequiredService<GitHubService>(), sp.GetRequiredService<AzureDevOpsService>()));
		builder.Services.AddTransient<RepositoriesViewModel>(sp =>
			new RepositoriesViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<IssuesViewModel>(sp =>
			new IssuesViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<CommitsViewModel>(sp =>
			new CommitsViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<PullRequestsViewModel>(sp =>
			new PullRequestsViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<PullRequestDetailViewModel>(sp =>
			new PullRequestDetailViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<CreatePullRequestViewModel>(sp =>
			new CreatePullRequestViewModel(sp.GetRequiredService<GitHubService>()));
		builder.Services.AddTransient<NotificationsViewModel>(sp =>
			new NotificationsViewModel(sp.GetRequiredService<GitHubService>()));
		
		// Register Pages
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<Views.RepositoriesPage>();
		builder.Services.AddTransient<Views.RepositoryDetailPage>();
		builder.Services.AddTransient<Views.IssuesPage>();
		builder.Services.AddTransient<Views.CommitsPage>();
		builder.Services.AddTransient<Views.PullRequestsPage>();
		builder.Services.AddTransient<Views.PullRequestDetailPage>();
		builder.Services.AddTransient<Views.CreatePullRequestPage>();
		builder.Services.AddTransient<Views.NotificationsPage>();
		builder.Services.AddTransient<Views.SettingsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
