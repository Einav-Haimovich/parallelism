namespace StockWatch;

class App(AppShell shell) : Application
{
	readonly AppShell _shell = shell;

	protected override Window CreateWindow(IActivationState? activationState) => new(_shell);
}