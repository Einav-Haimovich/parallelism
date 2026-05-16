using System.Diagnostics;

Trace.WriteLine($"Program Started on Thread {Environment.CurrentManagedThreadId}");

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.Zero);
var task = Task.Run(() => SimulateLongRunningFunction(CancellationToken.None));

Trace.WriteLine($"Is the Task completed? {task.IsCompleted}");
Trace.WriteLine($"Is the Task completed successfully? {task.IsCompletedSuccessfully}");
Trace.WriteLine($"Is the Task faulted? {task.IsFaulted}");
Trace.WriteLine($"Is the Task canceled? {task.IsCanceled}");

try
{
	await task; // Thread 1 will not be blocked and can now do other things
}
catch (Exception e)
{
	Trace.WriteLine(e);
}

Trace.WriteLine($"Is the Task completed? {task.IsCompleted}");
Trace.WriteLine($"Is the Task completed successfully? {task.IsCompletedSuccessfully}");
Trace.WriteLine($"Is the Task faulted? {task.IsFaulted}");
Trace.WriteLine($"Is the Task canceled? {task.IsCanceled}");

Trace.WriteLine("Program Completed");

async Task SimulateLongRunningFunction(CancellationToken token)
{
	Trace.WriteLine($"Started on Thread {Environment.CurrentManagedThreadId}");

	token.ThrowIfCancellationRequested();
	await Task.Delay(TimeSpan.FromSeconds(2), token); // Thread will return to the Thread Pool until Task.Delay is completed

	Trace.WriteLine($"Completed on Thread {Environment.CurrentManagedThreadId}");
}