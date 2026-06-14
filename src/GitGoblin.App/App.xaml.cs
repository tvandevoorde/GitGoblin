using Microsoft.Extensions.DependencyInjection;

namespace GitGoblin.App;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var mainPage = _serviceProvider.GetRequiredService<MainPage>();
		return new Window(new NavigationPage(mainPage));
	}
}