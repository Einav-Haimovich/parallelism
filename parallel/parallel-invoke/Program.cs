using System.Diagnostics;
using ParallelInvoke;

Trace.WriteLine("Cooking Started");

var turkey = new Turkey();
var gravy = new Gravy();
var mashedPotatoes = new MashedPotatoes();
var stuffing = new Stuffing();

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var options = new ParallelOptions
{
	CancellationToken = cts.Token,
	MaxDegreeOfParallelism = 2
};

try
{
	Parallel.Invoke(options, () => turkey.Cook(), () => gravy.Cook(), () => mashedPotatoes.Cook(), () => stuffing.Cook());
}
catch (OperationCanceledException)
{
	Trace.WriteLine("ERROR: Cooking took too long");
}


Trace.WriteLine("Cooking Complete");