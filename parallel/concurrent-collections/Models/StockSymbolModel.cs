namespace StockWatch;

public record StockSymbolModel(string Symbol, string CompanyName, Color Color, StockQuoteModel? Quote);