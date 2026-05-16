#pragma warning disable
using System.Diagnostics;

SemaphoreSlim semaphoreSlim = new(1, 1);
int loopIterations = 0;
bool result = false;

Task backgroundTask1 = Task.CompletedTask;
Task backgroundTask2 = Task.CompletedTask;

Trace.WriteLine($"Program Started on Thread {Environment.CurrentManagedThreadId}");

backgroundTask1 =
	Task.Run(() => SimulateLongRunningFunction("Background Task 1", backgroundTask2, CancellationToken.None));
backgroundTask2 =
	Task.Run(() => SimulateLongRunningFunction("Background Task 2", backgroundTask1, CancellationToken.None));

await backgroundTask1;
await backgroundTask2;

Trace.WriteLine("Program Completed");

async Task SimulateLongRunningFunction(string name, Task otherLongRunningFunction, CancellationToken token)
{
	Trace.WriteLine($"{name} started on Thread {Environment.CurrentManagedThreadId}");

	await Task.Delay(TimeSpan.FromSeconds(2), token);
	await otherLongRunningFunction;

	Trace.WriteLine($"{name} completed on Thread {Environment.CurrentManagedThreadId}");
}