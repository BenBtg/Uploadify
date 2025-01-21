namespace Uploadify.Maui;

public partial class App : Application
{
	MainPage _mainPage;
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		_mainPage = serviceProvider.GetService<MainPage>();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}