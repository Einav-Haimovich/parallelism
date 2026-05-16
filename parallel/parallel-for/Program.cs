using System.Diagnostics;
using ParallelFor;

const int numOrdersTurkey = 10;
const int numOrdersMashedPotatoes = 50;
const int numOrdersGravy = 50;
const int numOrdersStuffing = 20;

var turkey = new Turkey();
var mashedPotatoes = new MashedPotatoes();
var gravy = new Gravy();
var stuffing = new Stuffing();

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

var options = new ParallelOptions { CancellationToken = cancellationTokenSource.Token, MaxDegreeOfParallelism = 3 };

Trace.WriteLine("Cooking Started");

try
{
	var turkeyResult = Parallel.For(0, numOrdersTurkey, options, orderNumber => turkey.Cook(orderNumber));
	var mashedPotatoesResult = Parallel.For(0, numOrdersMashedPotatoes, options, orderNumber => mashedPotatoes.Cook(orderNumber));
	var gravyResult = Parallel.For(0, numOrdersGravy, options, orderNumber => gravy.Cook(orderNumber));
	var stuffingResult = Parallel.For(0, numOrdersStuffing, options, orderNumber => stuffing.Cook(orderNumber));

	if (turkeyResult.IsCompleted && mashedPotatoesResult.IsCompleted && gravyResult.IsCompleted && stuffingResult.IsCompleted)
		Trace.WriteLine("All Meals Complete");
	else
		Trace.WriteLine("Cooking Failed");
}
catch (OperationCanceledException)
{
	Trace.WriteLine("ERROR: Cooking took too long");
}