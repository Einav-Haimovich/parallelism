namespace StockWatch;

class LightTheme : BaseTheme
{
	public override Color PageBackgroundColor { get; } = Color.FromRgb(248, 249, 250);
	public override Color FlyoutBackgroundColor { get; } = Colors.White;

	public override Color PrimaryTextColor { get; } = Color.FromRgb(17, 24, 39);
	public override Color SecondaryTextColor { get; } = Color.FromRgb(99, 102, 241);

	public override Color SeparatorColor { get; } = Color.FromRgba(0, 0, 0, 0.08);
	public override Color CardBackgroundColor { get; } = Colors.White;
	public override Color CardBorderColor { get; } = Color.FromRgba(0, 0, 0, 0.05);
	public override Color HeaderBackgroundColor { get; } = Color.FromRgb(255, 255, 255);
}