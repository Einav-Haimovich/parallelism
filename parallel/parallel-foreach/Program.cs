using System.Diagnostics;
using ParallelForEach;

const int numOrdersTurkey = 10;
const int numOrdersMashedPotatoes = 50;
const int numOrdersGravy = 50;
const int numOrdersStuffing = 20;

ParallelLoopResult? turkeyResult = null, mashedPotatoesResult = null, gravyResult = null, stuffingResult = null;

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var options = new ParallelOptions { CancellationToken = cancellationTokenSource.Token, MaxDegreeOfParallelism = 40 };

var turkeyOrders =
	Enumerable.Range(1, numOrdersTurkey).Select(static _ => new Turkey());

var mashedPotatoesOrders =
	Enumerable.Range(1, numOrdersMashedPotatoes).Select(static _ => new MashedPotatoes());

var gravyOrders =
	Enumerable.Range(1, numOrdersGravy).Select(static _ => new Gravy());

var stuffingOrders =
	Enumerable.Range(1, numOrdersStuffing).Select(static _ => new Stuffing());

Trace.WriteLine("Cooking Started");

try
{
	turkeyResult = Parallel.ForEach(turkeyOrders, options, static (turkey, _, orderNumber) => turkey.Cook(orderNumber));
	mashedPotatoesResult = Parallel.ForEach(mashedPotatoesOrders, options, static (mashedPotatoes, _, orderNumber) => mashedPotatoes.Cook(orderNumber));
	gravyResult = Parallel.ForEach(gravyOrders, options, static (gravy, _, orderNumber) => gravy.Cook(orderNumber));
	stuffingResult = Parallel.ForEach(stuffingOrders, options, static (stuffing, _, orderNumber) => stuffing.Cook(orderNumber));
}
catch
{
	Trace.WriteLine("ERROR: Cooking took too long");
}

if (turkeyResult?.IsCompleted is true
	&& mashedPotatoesResult?.IsCompleted is true
	&& gravyResult?.IsCompleted is true
	&& stuffingResult?.IsCompleted is true)
{
	Trace.WriteLine("All Meals Complete");
}
else
{
	Trace.WriteLine("Unable to complete cooking");
}