using System.Net;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Refit;

namespace StockWatch;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		// Pages
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransientWithShellRoute<DashboardPage, DashboardViewModel>();

		// Services
		builder.Services.AddSingleton<FinnHubApiService>();
		builder.Services.AddRefitClient<IFinnHubApi>()
			.ConfigureHttpClient(static client => client.BaseAddress = new Uri(FinnHubConstants.ApiUrl))
			.ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.All
			});

		return builder.Build();
	}

	static IServiceCollection AddTransientWithShellRoute<TPage, TViewModel>(this IServiceCollection services) where TPage : BaseContentPage<TViewModel>, IShellRoutable
		where TViewModel : BaseViewModel
	{
		return services.AddTransientWithShellRoute<TPage, TViewModel>(TPage.Route);
	}

	sealed class MobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public MobileHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 3;
			UseJitter = true;
			Delay = TimeSpan.FromMilliseconds(200);
		}
	}
}