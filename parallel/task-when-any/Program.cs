using System.Diagnostics;
using TaskWhenAny;

Trace.WriteLine("Cooking Started");

var turkey = new Turkey();
var gravy = new Gravy();
var mashedPotatoes = new MashedPotatoes();
var stuffing = new Stuffing();

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(4));

try
{
	List<Task<string>> cookingList =
	[
		turkey.Cook(cancellationTokenSource.Token),
		gravy.Cook(cancellationTokenSource.Token),
		mashedPotatoes.Cook(cancellationTokenSource.Token),
		stuffing.Cook(cancellationTokenSource.Token),
	];

	while (cookingList.Count is not 0)
	{
		var completedCookingTask = await Task.WhenAny(cookingList);
		cookingList.Remove(completedCookingTask);

		var name = await completedCookingTask;

		Trace.WriteLine($"Eating {name}");
	}
}
catch (TaskCanceledException)
{
	Trace.WriteLine("ERROR: Cooking took too long");
}


Trace.WriteLine("Cooking Complete");