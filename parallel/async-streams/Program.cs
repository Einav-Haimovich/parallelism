using System.Diagnostics;
using TaskWhenAny;

Trace.WriteLine("Cooking Started");

var turkey = new Turkey();
var gravy = new Gravy();
var mashedPotatoes = new MashedPotatoes();
var stuffing = new Stuffing();

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
	List<Task<string>> cookingList =
	[
		turkey.Cook(cancellationTokenSource.Token),
		gravy.Cook(cancellationTokenSource.Token),
		mashedPotatoes.Cook(cancellationTokenSource.Token),
		stuffing.Cook(cancellationTokenSource.Token),
	];

	await foreach (var completedCookingTask in Task.WhenEach(cookingList).WithCancellation(cancellationTokenSource.Token))
	{
		var completedDish = await completedCookingTask;
		Trace.WriteLine($"Completed Cooking {completedDish}");
	}
}
catch (TaskCanceledException)
{
	Trace.WriteLine("ERROR: Cooking took too long");
}

Trace.WriteLine("Cooking Complete");