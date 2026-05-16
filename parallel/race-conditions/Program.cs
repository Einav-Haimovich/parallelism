#pragma warning disable
using System.Diagnostics;
using IntroToTask;

int loopIterations = 0;
int number = 0;

Trace.WriteLine($"Program Started on Thread {Environment.CurrentManagedThreadId}");

while (loopIterations < 1_000)
{
	var backgroundTask1 =
		Task.Run(() => SimulateLongRunningFunction("Background Task 1", 1, CancellationToken.None));
	var backgroundTask2 =
		Task.Run(() => SimulateLongRunningFunction("Background Task 2", 5, CancellationToken.None));

	await backgroundTask1;
	await backgroundTask2;

	loopIterations++;
}

Trace.WriteLine("Program Completed");

async Task SimulateLongRunningFunction(string name, int setNumber, CancellationToken token)
{
	Trace.WriteLine($"{name} started on Thread {Environment.CurrentManagedThreadId}");

	await Task.Delay(TimeSpan.FromMilliseconds(2));

	Interlocked.CompareExchange(ref number, setNumber, number);

	Trace.WriteLine($"{name} completed on Thread {Environment.CurrentManagedThreadId}");
}