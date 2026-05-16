using System.Diagnostics;
using TaskWhenAll;

Trace.WriteLine("Cooking Started");

var turkey = new Turkey();
var gravy = new Gravy();
var mashedPotatoes = new MashedPotatoes();
var stuffing = new Stuffing();

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(4));

try
{
	await Task.WhenAll(turkey.Cook(cancellationTokenSource.Token), gravy.Cook(cancellationTokenSource.Token), mashedPotatoes.Cook(cancellationTokenSource.Token), stuffing.Cook(cancellationTokenSource.Token));
}
catch (TaskCanceledException)
{
	Trace.WriteLine("ERROR: Cooking took too long");
}


Trace.WriteLine("Cooking Complete");