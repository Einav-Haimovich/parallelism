using System.Diagnostics;

namespace ParallelForAsync;

public abstract class Food
{
	readonly TimeSpan _cookTime;

	protected Food(TimeSpan cookTime)
	{
		_cookTime = cookTime;
		Name = GetType().Name;
	}

	public string Name { get; }

	public async Task Cook(int orderNumber, CancellationToken token)
	{
		Trace.WriteLine($"Cooking {Name} for Order Number {orderNumber} on Thread {Environment.CurrentManagedThreadId}");
		await Task.Delay(_cookTime, token);
		Trace.WriteLine($"{Name} Completed for Order Number {orderNumber} on Thread {Environment.CurrentManagedThreadId}");
	}
}

public class Turkey() : Food(TimeSpan.FromSeconds(5));
public class MashedPotatoes() : Food(TimeSpan.FromSeconds(2));
public class Gravy() : Food(TimeSpan.FromSeconds(1));
public class Stuffing() : Food(TimeSpan.FromSeconds(2));