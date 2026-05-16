using System.Diagnostics;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Markup;

namespace StockWatch;

class DashboardPage : BaseContentPage<DashboardViewModel>, IShellRoutable
{
	readonly ScrollView _scrollView;

	CancellationTokenSource? _autoScrollingCancellationTokenSource;

	public DashboardPage(DashboardViewModel dashboardViewModel) : base(dashboardViewModel)
	{
		Padding = 0;

		this.DynamicResource(BackgroundColorProperty, nameof(BaseTheme.PageBackgroundColor));

		Content = new ScrollView
		{
			Padding = new Thickness(12, 0),
			Content = new CollectionView
			{
				SelectionMode = SelectionMode.Single,
				ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					Span = GetGridSpan(),
					HorizontalItemSpacing = 0,
					VerticalItemSpacing = 0
				},
				ItemTemplate = new DataTemplate(() => new StockCardView())
			}
				.Bind(CollectionView.ItemsSourceProperty, nameof(DashboardViewModel.AssetList))
		}.Assign(out _scrollView);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		_autoScrollingCancellationTokenSource ??= new();
		StartAutoScroll(_autoScrollingCancellationTokenSource.Token).SafeFireAndForget(ex => Trace.WriteLine(ex));
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_autoScrollingCancellationTokenSource?.Cancel();
	}

	async Task StartAutoScroll(CancellationToken token)
	{
		await Task.Delay(1500, token);

		while (!token.IsCancellationRequested)
		{
			var ensureSixtyFramesPerSecondTask = Task.Delay(16, token);
			try
			{
				var currentY = _scrollView.ScrollY;
				var contentHeight = _scrollView.ContentSize.Height;
				var scrollViewHeight = _scrollView.Height;

				var targetY = currentY + 1.5;

				if (targetY >= contentHeight - scrollViewHeight)
				{
					await _scrollView.ScrollToAsync(0, 0, true).WaitAsync(token);
				}
				else
				{
					await _scrollView.ScrollToAsync(0, targetY, false).WaitAsync(token);
				}

				await ensureSixtyFramesPerSecondTask;
			}
			catch
			{
			}
		}
	}

	public static string Route => $"//{nameof(DashboardPage)}";

	static int GetGridSpan()
	{
		var width = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;

		return width switch
		{
			< 0 => throw new ArgumentOutOfRangeException(nameof(width)),
			< 600 => 2,
			< 900 => 3,
			< 1200 => 4,
			< 1600 => 5,
			_ => 6
		};
	}
}