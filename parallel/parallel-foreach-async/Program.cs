using System.Diagnostics;
using ParallelForEachAsync;

const int numOrdersTurkey = 10;
const int numOrdersMashedPotatoes = 50;
const int numOrdersGravy = 50;
const int numOrdersStuffing = 20;

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
	var turkeyCookingTasks = Parallel.ForEachAsync(turkeyOrders, options, static async (turkey, token) => await turkey.Cook(token));
	var mashedPotatoesTasks = Parallel.ForEachAsync(mashedPotatoesOrders, options, static async (mashedPotatoes, token) => await mashedPotatoes.Cook(token));
	var gravyCookingTasks = Parallel.ForEachAsync(gravyOrders, options, static async (gravy, token) => await gravy.Cook(token));
	var stuffingCookingTasks = Parallel.ForEachAsync(stuffingOrders, options, static async (stuffing, token) => await stuffing.Cook(token));

	await Task.WhenAll(turkeyCookingTasks, mashedPotatoesTasks, gravyCookingTasks, stuffingCookingTasks);

	Trace.WriteLine("All Meals Complete");
}
catch
{
	Trace.WriteLine("ERROR: Cooking took too long");
}
finally
{
	Trace.WriteLine("Cooking Ended");
}