using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace StockWatch;

class TickerView : CollectionView
{
	public const int OptimalHeight = 64;

	public TickerView()
	{
		this.CenterVertical();
		HeightRequest = OptimalHeight;
		SelectionMode = SelectionMode.Single;
		ItemsLayout = LinearItemsLayout.Horizontal;
		ItemTemplate = new StockTickerDataTemplate();
		HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
	}

	class StockTickerDataTemplate() : DataTemplate(CreateDataTemplate)
	{
		static Grid CreateDataTemplate() => new()
		{
			RowSpacing = 4,
			ColumnSpacing = 12,
			WidthRequest = 100,

			RowDefinitions = Rows.Define(
				(Row.Symbol, 16),
				(Row.Price, 20),
				(Row.PercentChange, 16),
				(Row.BottomPadding, 4)),

			ColumnDefinitions = Columns.Define(
				(Column.Separator, SeparatorView.RecommendedSeparatorViewSize),
				(Column.Content, Star)),

			Children =
			{
				new SeparatorView()
					.RowSpan(3).Column(Column.Separator)
					.Width(SeparatorView.RecommendedSeparatorViewSize),

				new Label()
					.Row(Row.Symbol).Column(Column.Content)
					.Font(size: 12)
					.Bind(Label.TextProperty,
						getter: static (StockSymbolModel model) => model.Symbol,
						convert: static (string? symbol) => symbol?.ToUpper(),
						mode: BindingMode.OneTime)
					.Bind(Label.TextColorProperty,
						getter: static (StockSymbolModel model) => model.Color,
						mode: BindingMode.OneTime),

				new Label()
					.Row(Row.Price).Column(Column.Content)
					.Font(size: 16)
					.Bind(Label.TextProperty,
						getter: static (StockSymbolModel model) => model.Quote,
						convert: static quote => quote?.CurrentPrice,
						mode: BindingMode.OneTime),

				new Label()
					.Row(Row.PercentChange).Column(Column.Content)
					.Font(size: 12)
					.Bind(Label.TextProperty,
						getter: static (StockSymbolModel model) => model.Quote,
						convert: static quote => quote is null ? "Unknown" : $"{quote?.PercentChange}%",
						mode: BindingMode.OneTime)
			}
		};

		enum Row
		{
			Symbol,
			Price,
			PercentChange,
			BottomPadding
		}

		enum Column
		{
			Separator,
			Content
		}
	}
}