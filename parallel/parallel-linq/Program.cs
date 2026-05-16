using System.Diagnostics;

IReadOnlyList<int> iterationsList = [.. Enumerable.Range(1, 10_000)];

Trace.WriteLine("***Finding prime numbers in series using LINQ...");
var seriesStopwatch = new Stopwatch();
seriesStopwatch.Start();

var primeNumbers = iterationsList.Select(FindNextPrimeNumber).ToList();
seriesStopwatch.Stop();

Trace.WriteLine($"***Finding prime numbers in series took {seriesStopwatch.Elapsed.TotalSeconds:F3}s.");

Trace.WriteLine("***Finding prime in Parallel using PLINQ...");
var parallelStopwatch = new Stopwatch();
parallelStopwatch.Start();

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var primeNumbersInParallel = iterationsList
										.AsParallel()
										.AsOrdered()
										.WithCancellation(cts.Token)
										.Select(FindNextPrimeNumber).ToList();

parallelStopwatch.Stop();

Trace.WriteLine($"***Finding prime numbers in parallel took {parallelStopwatch.Elapsed.TotalSeconds:F3}s.");

foreach (var primeNumber in primeNumbersInParallel)
{
	Trace.Write($"{primeNumber}, ");
}

static long FindNextPrimeNumber(int n)
{
	int count = 0;
	long a = 2;
	while (count < n)
	{
		long b = 2;
		int prime = 1;
		while (b * b <= a)
		{
			if (a % b is 0)
			{
				prime = 0;
				break;
			}

			b++;
		}

		if (prime > 0)
		{
			count++;
		}

		a++;
	}

	return (--a);
}