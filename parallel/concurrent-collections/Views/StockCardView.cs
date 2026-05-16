using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace StockWatch;

class StockCardView : Border
{
	public StockCardView()
	{
		this.Padding(20, 16)
			.Margin(12, 8);

		StrokeThickness = 0;
		StrokeShape = new RoundRectangle
		{
			CornerRadius = new CornerRadius(16)
		};

		Shadow = new Shadow
		{
			Brush = new SolidColorBrush(Colors.Black),
			Opacity = 0.15f,
			Radius = 12,
			Offset = new Point(0, 4)
		};

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.CardBackgroundColor));

		Content = new Grid
		{
			RowSpacing = 8,
			ColumnSpacing = 12,

			RowDefinitions = Rows.Define(
				(Row.Symbol, Auto),
				(Row.CompanyName, Auto),
				(Row.Price, Auto),
				(Row.Change, Auto),
				(Row.PercentChange, Auto),
				(Row.Indicator, 3)),

			ColumnDefinitions = Columns.Define(
				(Column.Main, Star)),

			Children =
			{
				new Label()
					.Row(Row.Symbol).Column(Column.Main)
					.Font(size: 22, bold: true)
					.CenterHorizontal()
					.Bind(Label.TextProperty,
						getter: static (StockSymbolModel model) => model.Symbol,
						mode: BindingMode.OneTime)
					.Bind(Label.TextColorProperty,
						getter: static (StockSymbolModel model) => model.Color,
						mode: BindingMode.OneWay),

				new Label
				{
					FontSize = 11,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.TailTruncation,
					MaxLines = 1,
					Opacity = 0.7
				}
				.Row(Row.CompanyName).Column(Column.Main)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
				.Bind(Label.TextProperty,
					getter: static (StockSymbolModel model) => model.CompanyName,
					mode: BindingMode.OneTime),

				new Label
				{
					FontSize = 28,
					FontAttributes = FontAttributes.Bold,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.NoWrap,
					MaxLines = 1
				}
				.Row(Row.Price).Column(Column.Main)
				.DynamicResource(Label.TextColorProperty, nameof(BaseTheme.PrimaryTextColor))
				.Bind(Label.TextProperty,
					getter: static (StockSymbolModel model) => model.Quote,
					convert: static quote => quote is null ? "Loading..." : $"${quote.CurrentPrice:F2}",
					mode: BindingMode.OneWay),

				new Label
				{
					FontSize = 15,
					FontAttributes = FontAttributes.Bold,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.NoWrap,
					MaxLines = 1
				}
				.Row(Row.Change).Column(Column.Main)
				.Bind(Label.TextProperty,
					getter: static (StockSymbolModel model) => model.Quote,
					convert: static quote => quote is null ? "..." : $"{(quote.Change >= 0 ? "▲" : "▼")} ${Math.Abs(quote.Change):F2}",
					mode: BindingMode.OneWay)
				.Bind(Label.TextColorProperty,
					getter: static (StockSymbolModel model) => model.Color,
					mode: BindingMode.OneWay),

				new Label
				{
					FontSize = 15,
					HorizontalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.NoWrap,
					MaxLines = 1
				}
				.Row(Row.PercentChange).Column(Column.Main)
				.Bind(Label.TextProperty,
					getter: static (StockSymbolModel model) => model.Quote,
					convert: static quote => quote is null ? "..." : $"{(quote.PercentChange >= 0 ? "+" : "")}{quote.PercentChange:F2}%",
					mode: BindingMode.OneWay)
				.Bind(Label.TextColorProperty,
					getter: static (StockSymbolModel model) => model.Color,
					mode: BindingMode.OneWay),

				new BoxView()
					.Row(Row.Indicator).Column(Column.Main)
					.Height(3)
					.Bind(BoxView.ColorProperty,
						getter: static (StockSymbolModel model) => model.Color,
						mode: BindingMode.OneWay)
			}
		};
	}

	enum Row
	{
		Symbol,
		CompanyName,
		Price,
		Change,
		PercentChange,
		Indicator
	}

	enum Column
	{
		Main
	}
}