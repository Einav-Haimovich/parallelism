using System.Text.Json.Serialization;

namespace StockWatch;

public record StockQuoteModel(
	[property: JsonPropertyName("c")] double CurrentPrice,
	[property: JsonPropertyName("h")] double DayHighPrice,
	[property: JsonPropertyName("l")] double DayLowPrice,
	[property: JsonPropertyName("o")] double OpenPrice,
	[property: JsonPropertyName("pc")] double PreviousClosePrice,
	[property: JsonPropertyName("dp")] double PercentChange,
	[property: JsonPropertyName("d")] double Change,
	[property: JsonPropertyName("t")] long Timestamp)
{
	public DateTimeOffset Time => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
}