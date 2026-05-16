using Refit;

namespace StockWatch;

public class FinnHubApiService(IFinnHubApi finnHubApi)
{
	readonly IFinnHubApi _finnHubApi = finnHubApi;

	public Task<StockQuoteModel> GetStockQuote(string symbol, CancellationToken token) => _finnHubApi.GetStockQuote(symbol, FinnHubConstants.ApiKey, token);
}

[Headers("User-Agent: " + nameof(StockWatch), "Accept-Encoding: gzip", "Accept: application/json")]
public interface IFinnHubApi
{
	[Get("/quote")]
	Task<StockQuoteModel> GetStockQuote(string symbol, [Header("X-Finnhub-Token")] string apiKey, CancellationToken token);
}