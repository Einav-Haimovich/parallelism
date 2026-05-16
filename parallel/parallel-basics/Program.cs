using System.Diagnostics;

Trace.WriteLine($"Program Started on Thread {Environment.CurrentManagedThreadId}");

var backgroundTask1 = Task.Run(() => SimulateLongRunningFunction("Background Task 1", CancellationToken.None));
var backgroundTask2 = Task.Run(() => SimulateLongRunningFunction("Background Task 2", CancellationToken.None));

await backgroundTask1;
await backgroundTask2;

Trace.WriteLine("Program Completed");

async Task SimulateLongRunningFunction(string name, CancellationToken token)
{
	Trace.WriteLine($"{name} started on Thread {Environment.CurrentManagedThreadId}");

	await Task.Delay(TimeSpan.FromSeconds(2), token);

	Trace.WriteLine($"{name} completed on Thread {Environment.CurrentManagedThreadId}");
}