using System.Diagnostics;
using ParallelForAsync;

const int numOrdersTurkey = 10;
const int numOrdersMashedPotatoes = 50;
const int numOrdersGravy = 50;
const int numOrdersStuffing = 20;

var turkey = new Turkey();
var gravy = new Gravy();
var stuffing = new Stuffing();
var mashedPotatoes = new MashedPotatoes();

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var options = new ParallelOptions { CancellationToken = cancellationTokenSource.Token, MaxDegreeOfParallelism = 40 };

Trace.WriteLine("Cooking Started");

try
{
	var cookTurkeysTask = Parallel.ForAsync(0, numOrdersTurkey, options, async (orderNumber, token) => await turkey.Cook(orderNumber, token));
	var cookGravyTask = Parallel.ForAsync(0, numOrdersGravy, options, async (orderNumber, token) => await gravy.Cook(orderNumber, token));
	var cookStuffingTask = Parallel.ForAsync(0, numOrdersStuffing, options, async (orderNumber, token) => await stuffing.Cook(orderNumber, token));
	var cookMashedPotatoesTask = Parallel.ForAsync(0, numOrdersMashedPotatoes, options, async (orderNumber, token) => await mashedPotatoes.Cook(orderNumber, token));

	await Task.WhenAll(cookTurkeysTask, cookMashedPotatoesTask, cookGravyTask, cookStuffingTask);

	Trace.WriteLine("All Meals Complete");
}
catch
{
	Trace.WriteLine("ERROR: Cooking took too long");
}