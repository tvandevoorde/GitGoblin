namespace GitGoblin.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("RepositoryDetailPage", typeof(Views.RepositoryDetailPage));
		Routing.RegisterRoute("IssuesPage", typeof(Views.IssuesPage));
		Routing.RegisterRoute("CommitsPage", typeof(Views.CommitsPage));
		Routing.RegisterRoute("PullRequestsPage", typeof(Views.PullRequestsPage));
		Routing.RegisterRoute("PullRequestDetailPage", typeof(Views.PullRequestDetailPage));
		Routing.RegisterRoute("CreatePullRequestPage", typeof(Views.CreatePullRequestPage));
	}
}
