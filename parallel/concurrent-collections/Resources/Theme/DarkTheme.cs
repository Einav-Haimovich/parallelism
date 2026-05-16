namespace StockWatch;

class DarkTheme : BaseTheme
{
	public override Color PageBackgroundColor { get; } = Color.FromRgb(15, 23, 42);
	public override Color FlyoutBackgroundColor { get; } = Color.FromRgb(30, 41, 59);

	public override Color PrimaryTextColor { get; } = Color.FromRgb(248, 250, 252);
	public override Color SecondaryTextColor { get; } = Color.FromRgb(129, 140, 248);

	public override Color SeparatorColor { get; } = Color.FromRgba(255, 255, 255, 0.08);
	public override Color CardBackgroundColor { get; } = Color.FromRgb(30, 41, 59);
	public override Color CardBorderColor { get; } = Color.FromRgba(255, 255, 255, 0.06);
	public override Color HeaderBackgroundColor { get; } = Color.FromRgb(30, 41, 59);
}