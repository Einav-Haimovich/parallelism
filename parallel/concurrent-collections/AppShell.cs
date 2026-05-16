namespace StockWatch;

class AppShell : Shell
{
	public AppShell(DashboardPage dashboardPage)
	{
		Items.Add(dashboardPage);
	}
}