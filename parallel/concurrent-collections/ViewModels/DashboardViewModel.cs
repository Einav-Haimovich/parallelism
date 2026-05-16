using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using AsyncAwaitBestPractices;
using Refit;

namespace StockWatch;

public sealed partial class DashboardViewModel : BaseViewModel, IAsyncDisposable
{
	readonly FinnHubApiService _finnHubApiService;
	readonly ConcurrentDictionary<string, StockQuoteModel> _latestStockQuotes = new();
	readonly SemaphoreSlim _retrievingStockDataRaceConditionSemaphoreSlim = new(1, 1);

	readonly IReadOnlyDictionary<string, string> _stockSymbols = new Dictionary<string, string>
	{
		{ "AAPL", "Apple Inc." },
		{ "MSFT", "Microsoft Corp." },
		{ "GOOGL", "Alphabet Inc." },
		{ "AMZN", "Amazon.com Inc." },
		{ "NVDA", "NVIDIA Corp." },
		{ "META", "Meta Platforms" },
		{ "TSLA", "Tesla Inc." },
		{ "BRK.B", "Berkshire Hathaway" },
		{ "LLY", "Eli Lilly & Co." },
		{ "AVGO", "Broadcom Inc." },
		{ "V", "Visa Inc." },
		{ "JPM", "JPMorgan Chase" },
		{ "WMT", "Walmart Inc." },
		{ "XOM", "Exxon Mobil" },
		{ "UNH", "UnitedHealth Group" },
		{ "MA", "Mastercard Inc." },
		{ "ORCL", "Oracle Corp." },
		{ "HD", "Home Depot Inc." },
		{ "PG", "Procter & Gamble" },
		{ "COST", "Costco Wholesale" },
		{ "JNJ", "Johnson & Johnson" },
		{ "NFLX", "Netflix Inc." },
		{ "ABBV", "AbbVie Inc." },
		{ "BAC", "Bank of America" },
		{ "CRM", "Salesforce Inc." },
		{ "MRK", "Merck & Co." },
		{ "CVX", "Chevron Corp." },
		{ "KO", "Coca-Cola Co." },
		{ "ADBE", "Adobe Inc." },
		{ "AMD", "Advanced Micro" },
	};

	Timer? _getStockDataTimer;

	public DashboardViewModel(FinnHubApiService finnHubApiService)
	{
		_finnHubApiService = finnHubApiService;
		_getStockDataTimer = CreateGetStockDataTimer();
	}

	public IReadOnlyList<StockSymbolModel> AssetList => GetStockSymbols();

	public async ValueTask DisposeAsync()
	{
		await (_getStockDataTimer?.DisposeAsync() ?? ValueTask.CompletedTask);
		_getStockDataTimer = null;
	}

	Timer CreateGetStockDataTimer()
	{
		var timer = new Timer(async _ =>
		{
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			await UpdateStockPrices(cts.Token).ConfigureAwait(false);
		});
		timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

		return timer;
	}

	async Task UpdateStockPrices(CancellationToken token)
	{
		try
		{
			await Parallel.ForEachAsync(_stockSymbols.Keys, token, async (symbol, token) => await UpdateStockPrice(symbol, token));
		}
		catch (OperationCanceledException)
		{
			await StopRetrievingStockData(token).ConfigureAwait(false);
		}
	}

	async ValueTask StopRetrievingStockData(CancellationToken token)
	{
		if (_getStockDataTimer is null)
		{
			return;
		}

		await _retrievingStockDataRaceConditionSemaphoreSlim.WaitAsync(token);

		try
		{
			if (_getStockDataTimer is not null)
			{
				await _getStockDataTimer.DisposeAsync();
				_getStockDataTimer = null;
			}
		}
		finally

		{
			_retrievingStockDataRaceConditionSemaphoreSlim.Release();
		}
	}

	async ValueTask StartRetrievingStockData(CancellationToken token)
	{
		if (_getStockDataTimer is not null)
		{
			return;
		}

		await _retrievingStockDataRaceConditionSemaphoreSlim.WaitAsync(token);

		try
		{
			await StopRetrievingStockData(token);
			_getStockDataTimer = CreateGetStockDataTimer();
		}
		finally
		{
			_retrievingStockDataRaceConditionSemaphoreSlim.Release();
		}
	}

	async Task UpdateStockPrice(string symbol, CancellationToken token)
	{
		try
		{
			var quote = await _finnHubApiService.GetStockQuote(symbol, token).ConfigureAwait(false);
			_latestStockQuotes.AddOrUpdate(symbol, _ => quote, (_, previousQuote) => quote.Time > previousQuote.Time ? quote : previousQuote);

			Trace.WriteLine($"Updated Stock Price for {quote}");

			OnPropertyChanged(nameof(AssetList));
		}
		catch (ApiException e) when (e.StatusCode is HttpStatusCode.TooManyRequests)
		{
			if (e.Headers.TryGetValues("X-Ratelimit-Reset", out var rateLimitResetHeaders)
				&& long.TryParse(rateLimitResetHeaders.Single(), out var rateLimitResetDateTimeInUnixTimeSeconds))
			{
				var rateLimitDuration = DateTimeOffset.FromUnixTimeSeconds(rateLimitResetDateTimeInUnixTimeSeconds) - DateTimeOffset.UtcNow;

				await StopRetrievingStockData(CancellationToken.None).ConfigureAwait(false);

				await Task.Delay(rateLimitDuration, CancellationToken.None).ConfigureAwait(false);

				await StartRetrievingStockData(CancellationToken.None).ConfigureAwait(false);
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	IReadOnlyList<StockSymbolModel> GetStockSymbols()
	{
		ConcurrentBag<StockSymbolModel> stockSymbolList = [];

		Parallel.ForEach(_stockSymbols, pair =>
		{
			var (symbol, companyName) = pair;
			_latestStockQuotes.TryGetValue(symbol, out var latestStockQuote);

			var color = latestStockQuote switch
			{
				null => Colors.Grey,
				{ Change: > 0 } => Color.FromRgb(9, 133, 81),
				{ Change: < 0 } => Color.FromRgb(207, 32, 47),
				_ => Colors.Grey
			};

			stockSymbolList.Add(new StockSymbolModel(symbol, companyName, color, latestStockQuote));
		});

		return [.. stockSymbolList.OrderBy(static x => x.Symbol)];
	}
}