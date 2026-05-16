using System.Diagnostics;

Trace.WriteLine($"Program Started on Thread {Environment.CurrentManagedThreadId}");

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.Zero);
var task = Task.Run(() => SimulateLongRunningFunction(cancellationTokenSource.Token), cancellationTokenSource.Token);

Trace.WriteLine($"What is the Task status? {task.Status}");

Trace.WriteLine($"Is the Task completed? {task.IsCompleted}");
Trace.WriteLine($"Is the Task completed successfully? {task.IsCompletedSuccessfully}");
Trace.WriteLine($"Is the Task faulted? {task.IsFaulted}");
Trace.WriteLine($"Is the Task canceled? {task.IsCanceled}");

Thread.Sleep(TimeSpan.FromSeconds(3));

Trace.WriteLine($"What is the Task status? {task.Status}");

Trace.WriteLine($"Is the Task completed? {task.IsCompleted}");
Trace.WriteLine($"Is the Task completed successfully? {task.IsCompletedSuccessfully}");
Trace.WriteLine($"Is the Task faulted? {task.IsFaulted}");
Trace.WriteLine($"Is the Task canceled? {task.IsCanceled}");

Trace.WriteLine("Program Completed");

void SimulateLongRunningFunction(CancellationToken token)
{
	Trace.WriteLine($"Long running function started on Thread {Environment.CurrentManagedThreadId}");

	token.ThrowIfCancellationRequested();
	Thread.Sleep(TimeSpan.FromSeconds(2));

	Trace.WriteLine($"Long running function completed on Thread {Environment.CurrentManagedThreadId}");
}