# Parallel Programming

Parallel programming is about using all available CPU cores to complete compute-intensive work faster. This repository covers the full breadth of .NET's parallel toolkit: the conceptual split between asynchronous I/O and true CPU parallelism, the Task primitives that coordinate concurrent work, the Parallel class for structured data parallelism, thread-safe data structures for shared state, channels for decoupled producer-consumer pipelines, and Parallel LINQ for distributing query work — each topic grounded in focused, runnable C# examples.

---

### 1. Parallel vs Asynchronous

This section establishes the conceptual and practical boundary between asynchronous and parallel programming in C#. Asynchronous programming — built on `Task`, `async`, and `await` — is primarily about freeing threads from blocking while waiting on I/O or time-bound operations, keeping applications responsive without consuming thread-pool resources unnecessarily. Parallel programming, by contrast, is about executing multiple units of CPU-bound work simultaneously across several threads. The section uses hands-on examples to show both styles in action, then immediately introduces the failure modes that arise once multiple threads share state: deadlocks and race conditions. By the end, readers understand not just how to launch concurrent work, but what can go wrong and how to prevent it.

**What I learned:**
- Asynchronous programming and parallel programming both use `Task` and threads, but serve different purposes: async is about non-blocking I/O and UI responsiveness, while parallel is about doing more CPU work simultaneously. Confusing the two leads to either wasted threads or missed performance gains.
- `Thread.Sleep` is a blocking call that locks the calling thread, preventing it from returning to the thread pool. Replacing it with `await Task.Delay` releases the thread while waiting, which is essential for scalability — a server fielding thousands of concurrent requests will exhaust its thread pool if every handler holds a thread while waiting.
- The `await` keyword does two things that are easy to overlook: it yields the calling thread back to the thread pool (non-blocking), and it automatically re-throws any exception that occurred inside the awaited task, eliminating the need to manually inspect `IsFaulted`.
- Tasks begin executing as soon as they are created via `Task.Run` — the `await` keyword does not start a task, it only suspends the current method until the task finishes. This means two tasks can run concurrently simply by starting both before awaiting either.
- A deadlock arises when two tasks each await the other's completion, forming a circular dependency from which neither can escape. Circular task dependencies must be identified at design time because they produce no error message — just a hung program.
- Shared mutable state accessed by multiple threads produces race conditions even in code that looks correct in single-threaded review. The three remedies in order of preference: `lock` for synchronous code, `SemaphoreSlim` (with `WaitAsync`) for async-compatible code, and `Interlocked.CompareExchange` for the highest-performance cases involving numeric values.
- Singleton initialization is itself a race condition hazard when two threads first access the singleton simultaneously. `Lazy<T>` solves this because .NET guarantees its initialization is thread-safe, making it the correct pattern for any singleton that may be accessed from parallel code.

---

### 2. The Task Class

This section covers the three primary methods .NET provides for running multiple Tasks concurrently: `Task.WhenAll`, `Task.WhenAny`, and `Task.WhenEach`. Rather than treating these as simple API calls, the section explores the behavioral differences between them — specifically, how each controls when your code continues execution relative to the completion of background work. A running cooking simulation (turkey, gravy, mashed potatoes, stuffing, each with distinct cook times) makes the timing differences concrete and observable.

**What I learned:**
- `Task.WhenAll` runs all passed tasks concurrently and suspends the caller until every task finishes. Total elapsed time equals the duration of the slowest task, not the sum — the cooking demo shows all four dishes starting simultaneously, with completion order determined by individual cook times.
- `Task.WhenAny` returns as soon as the first task in the list completes, leaving the remaining tasks still running on background threads. To process all results incrementally, you must drive it with a `while` loop that removes each completed task from the list, which is functional but produces noisy code.
- `Task.WhenEach` replaces the `Task.WhenAny` while-loop pattern with an `await foreach`, letting .NET manage task tracking internally and producing significantly cleaner, more maintainable code.
- Tasks start executing at the moment they are created, not when they are awaited. Adding a task to a list is enough to begin its background execution — the list construction and the `await` are separate concerns.
- `Task.WhenAll` and `Task.WhenAny` do not accept a `CancellationToken` directly. The recommended approach is to either pass the token into each individual task method, or bolt `.WaitAsync(token)` onto the aggregate task. For `IAsyncEnumerable` used with `Task.WhenEach`, the equivalent is the `.WithCancellation(token)` extension method.
- When a cancellation token fires against `Task.WhenAny` without also being passed into the individual child tasks, those background tasks continue running after the exception is thrown. Passing the token into each child task ensures all background work stops when the timeout fires.
- Calling `.Result` on a completed task returned by `Task.WhenAny` is technically safe (the task is guaranteed finished), but always using `await` instead prevents the pattern from being copied to contexts where `.Result` would deadlock the calling thread.

---

### 3. The Parallel Class

This section introduces the `Parallel` class from `System.Threading.Tasks`, which provides structured, high-level abstractions for running CPU-bound work across multiple threads without manually managing tasks. Rather than requiring the programmer to create and coordinate `Task` objects directly, the `Parallel` class handles thread distribution automatically while still offering precise control over concurrency limits and cancellation. The section works through five methods — `Parallel.Invoke`, `Parallel.For`, `Parallel.ForAsync`, `Parallel.ForEach`, and `Parallel.ForEachAsync` — each demonstrated through a cooking simulation that runs discrete units of work in parallel.

**What I learned:**
- `Parallel.Invoke`, `Parallel.For`, and `Parallel.ForEach` are all blocking calls: the calling thread is held until all parallel work finishes. In a UI application this means the main thread is frozen for the duration, making the synchronous variants unsafe for interactive apps.
- The `async` counterparts (`Parallel.ForAsync` and `Parallel.ForEachAsync`) return a `Task` and can be awaited, releasing the calling thread back to the runtime while background work proceeds.
- `ParallelOptions` is the unified configuration object for the entire `Parallel` class. Setting `MaxDegreeOfParallelism` caps how many threads run concurrently, which prevents thread-pool exhaustion when the total number of work items far exceeds available CPU cores.
- Cancellation tokens passed through `ParallelOptions` propagate to all iterations. The synchronous methods cancel abruptly and throw `OperationCanceledException`; the async methods allow the token to be forwarded into each async body so individual work items can cancel gracefully mid-operation.
- `Parallel.ForEach` provides a `ParallelLoopState` parameter inside its body. Calling `state.Break()` signals the loop to stop scheduling new iterations beyond the current index, while `state.Stop()` halts all further scheduling unconditionally. The returned `ParallelLoopResult.LowestBreakIteration` is non-null only after `Break`, not after `Stop`.
- `Parallel.Invoke` is preferred over `Task.WaitAll` for void-returning methods because it applies internal performance optimizations and surfaces the actual exception type rather than wrapping it in an `AggregateException`.

---

### 4. Concurrent Collections

This section covers the thread-safe collection types in `System.Collections.Concurrent` and the reasoning process for knowing when and why to use them. Rather than treating these as drop-in replacements for their single-threaded equivalents, the section teaches a mental model: trace how many threads touch a shared data structure, in what direction (read vs. write), and how frequently — then choose the collection whose internal locking strategy matches that access pattern. The concepts are grounded in a real .NET MAUI stock-ticker app (`StockWatch`) where a recurring timer fires `Parallel.ForEachAsync` across ~30 stock symbols every two seconds, creating genuine multi-threaded read/write pressure on shared state.

**What I learned:**
- `ConcurrentDictionary` uses per-key locking rather than a single global lock, so concurrent writes to distinct keys proceed without contention; only threads writing to the same key are serialized. `AddOrUpdate` with a factory delegate performs this as a single atomic operation, replacing the error-prone try-add-then-overwrite pattern.
- `ConcurrentStack` and `ConcurrentQueue` are fully lock-free because they use `Interlocked.Exchange` internally, making them cheaper than lock-based alternatives for high-throughput push/pop or enqueue/dequeue workloads under parallel access.
- `ConcurrentBag` is optimized for the case where the same thread both writes and reads an item; when a different thread reads what another thread wrote, locking occurs, making it the least generally useful of the four.
- Concurrent collections do not solve every shared-state problem. A shared mutable reference accessed by dozens of parallel threads required `SemaphoreSlim(1,1)` rather than a concurrent collection — illustrating that the right tool depends on whether the shared resource is a collection or an arbitrary object.
- Wrapping a `SemaphoreSlim` critical section in `try/finally` with `Release()` in the `finally` block is essential: an unhandled exception inside the guarded block would otherwise leave the semaphore permanently acquired, causing a deadlock on every subsequent caller.
- `BlockingCollection` is deliberately excluded because it predates `async`/`await` and blocks its calling thread; channels are the modern replacement for producer-consumer pipelines.

---

### 5. Channels

This section introduces `System.Threading.Channels` as the most capable producer/consumer mechanism in .NET, explaining how a shared in-memory buffer decouples the threads that produce work from the threads that consume it. The section covers three channel varieties — unbounded, unbounded-priority, and bounded — and walks through a full restaurant simulation where multiple concurrent waiters write food orders into a bounded channel while a single cook reads and processes them, finishing any remaining queue after the restaurant closes.

**What I learned:**
- A bounded channel applies back-pressure automatically: when the buffer is full, `WriteAsync` awaits instead of overflowing memory or dropping data, which makes `BoundedChannelFullMode.Wait` the safest default for systems where every item must be processed.
- Unbounded channels have no enforced size limit, but are still bounded by physical memory — appropriate for low-volume workloads, but bounded channels are preferred for production code where runaway growth could crash the process.
- An unbounded priority channel reorders reads by delegating to .NET's built-in `PriorityQueue` via `IComparer<T>`; the priority values are relative integers, so what matters is their ordering relationship, not their absolute magnitude.
- `ReadAllAsync` returns an `IAsyncEnumerable<T>` that drives an `await foreach` loop, keeping a reader perpetually waiting for the next item without polling — the loop exits naturally only after `Writer.Complete()` is called and the buffer is fully drained.
- Calling `Writer.Complete()` is mandatory cleanup: it signals the channel that no further writes will occur, which allows `ReadAllAsync` consumers to finish rather than await indefinitely.
- Setting `SingleReader = true` and `SingleWriter = false` lets the channel skip certain synchronization overhead internally; a single-reader channel consistently processes items on the same thread, matching an intended single-consumer scenario.

---

### 6. Parallel LINQ

Parallel LINQ (PLINQ) extends ordinary LINQ queries with data parallelism by distributing work across multiple CPU cores with minimal code changes. This section explores how adding `.AsParallel()` to a LINQ chain signals to the .NET runtime that a query may benefit from parallel execution — though the runtime retains the right to fall back to sequential processing when parallelism would cost more than it saves. Using a computationally expensive prime-number finder applied to 10,000 inputs as a benchmark, the section demonstrates the practical speedup PLINQ can achieve and walks through the key configuration knobs that control ordering, cancellation, buffering, and degree of concurrency.

**What I learned:**
- PLINQ parallelizes a LINQ query by inserting `.AsParallel()` into the chain, but the runtime makes an adaptive decision about whether to actually run in parallel based on the size and cost of the workload — parallelism is not unconditionally guaranteed.
- Because parallel threads complete in non-deterministic order, results are unordered by default; `.AsOrdered()` preserves source-sequence ordering at the cost of additional coordination overhead, and `.AsUnordered()` explicitly opts out of that guarantee.
- Passing a `CancellationToken` via `.WithCancellation()` is a first-class concern in PLINQ, especially for long-running queries where a timeout budget is required to avoid indefinite blocking.
- Degrees of parallelism can be capped with `.WithDegreeOfParallelism()` to prevent a single query from exhausting the thread pool and starving other concurrent work in the application.
- `.WithMergeOptions()` controls when results are flushed to the consumer: `NoBuffer` yields results immediately, `FullyBuffered` waits until all work completes, and `AutoBuffered` (the default) lets the runtime choose a buffer size for throughput balance.
- `.WithExecutionMode(ParallelExecutionMode.ForceParallelism)` overrides the runtime's cost analysis and mandates parallel execution; this should only be used when independent benchmarks confirm the runtime's default choice is wrong.

---

## How to Run

Each project is a standalone .NET console app (except `concurrent-collections`, which is a .NET MAUI app). To run any console example:

```sh
cd parallel/<project-name>
dotnet run
```

Open `parallel.slnx` in Visual Studio 2022 or JetBrains Rider to browse all 16 projects in a single solution.

---

## Folder Structure

```
parallel/
  async-streams/
  async-vs-parallel/
  channels/
  concurrent-collections/
  deadlocks/
  parallel-basics/
  parallel-for/
  parallel-for-async/
  parallel-foreach/
  parallel-foreach-async/
  parallel-invoke/
  parallel-linq/
  race-conditions/
  task-basics/
  task-when-all/
  task-when-any/
```

---

Thanks to Brandon Minnick for the course — [From Zero to Hero: Parallel Programming in C#](https://dometrain.com/course/from-zero-to-hero-parallel-programming-in-csharp/).

[Certificate of completion](<certificate/Parallel Programming in C# - Einav Haimovich.pdf>)
