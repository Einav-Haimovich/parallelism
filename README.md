# Parallel Programming

Parallel programming is about making CPU-bound work faster by using all available cores at once. This repo covers the full spectrum: the difference between parallelism and async/await, the core Task primitives for coordinating concurrent work, the Parallel class for data parallelism, thread-safe data structures, producer-consumer channels, and parallel LINQ — each with a focused example showing when and why each tool exists.

---

## Fundamentals

## [task-basics](parallel/task-basics/)

What a Task is and how to create, start, and wait on one directly.

_Learned: a Task is just a wrapper around a unit of work — understanding it at this level makes all the higher-level APIs (WhenAll, Parallel.For, PLINQ) feel obvious rather than magical._

---

## [async-vs-parallel](parallel/async-vs-parallel/)

Side-by-side comparison of async/await and parallel execution on the same problem.

_Learned: async frees a thread while waiting (I/O-bound work); parallel uses multiple threads at once (CPU-bound work) — confusing the two wastes resources or misses the performance gain entirely._

---

## [parallel-basics](parallel/parallel-basics/)

First look at running work in parallel: multiple threads, same process.

_Learned: spinning up threads has overhead — parallelism only pays off when the per-unit work is large enough to outweigh the coordination cost._

---

## [deadlocks](parallel/deadlocks/)

Demonstrates how deadlocks form in multithreaded code.

_Learned: deadlocks happen when two threads each hold a lock the other needs — the fix is to always acquire locks in a consistent order and never hold a lock while awaiting an async operation._

---

## [race-conditions](parallel/race-conditions/)

What happens when two threads read-modify-write the same variable without synchronization.

_Learned: a race condition produces non-deterministic results that can't be reliably reproduced — the fix is either synchronization (lock, Interlocked) or eliminating shared mutable state entirely._

---

## Task Coordination

## [task-when-all](parallel/task-when-all/)

Starts multiple tasks simultaneously and waits for all of them to finish.

_Learned: total time equals the slowest task, not the sum — and a single failure fails the whole batch, so handle exceptions from each task individually if partial results matter._

---

## [task-when-any](parallel/task-when-any/)

Returns as soon as the first task completes.

_Learned: `WhenAny` is the primitive for timeout patterns and "first wins" scenarios — useful when you have multiple paths to the same result and want whichever arrives first._

---

## [async-streams](parallel/async-streams/)

`IAsyncEnumerable<T>` for producing and consuming items one at a time from an async source.

_Learned: async streams decouple production from consumption — items flow through the pipeline as they're ready, so memory stays flat even for infinite or very large sequences._

---

## Parallel Class

## [parallel-invoke](parallel/parallel-invoke/)

Runs a fixed set of independent delegates in parallel.

_Learned: the simplest parallel primitive — use it when you have a known set of independent operations and just want them to run concurrently without managing tasks yourself._

---

## [parallel-for](parallel/parallel-for/)

Parallel version of a numeric for loop.

_Learned: `Parallel.For` partitions iterations across thread pool threads automatically — effective for CPU-bound loops where each iteration is independent, but the per-iteration cost must outweigh scheduling overhead._

---

## [parallel-for-async](parallel/parallel-for-async/)

Parallel for loop with an async body.

_Learned: bridges the parallel and async worlds — use it when your loop body is I/O-bound (HTTP calls, DB queries) and you want bounded concurrency without managing a semaphore manually._

---

## [parallel-foreach](parallel/parallel-foreach/)

Parallel iteration over any `IEnumerable`.

_Learned: same tradeoffs as `Parallel.For` but for collections — degree of parallelism defaults to processor count, which is the right default for pure CPU work._

---

## [parallel-foreach-async](parallel/parallel-foreach-async/)

Async parallel foreach with configurable concurrency.

_Learned: `MaxDegreeOfParallelism` lets you throttle concurrent I/O operations — critical when the downstream resource (an API, a database) has its own limits that parallel execution would otherwise blow past._

---

## Data Structures

## [concurrent-collections](parallel/concurrent-collections/)

Thread-safe collections used in a real-time StockWatch app.

_Learned: concurrent collections eliminate explicit locks on most operations by using fine-grained internal locking and compare-and-swap — faster than wrapping a regular collection in a lock, and composable without deadlock risk._

---

## [channels](parallel/channels/)

Producer-consumer pipeline using typed, bounded channels.

_Learned: a channel is a queue designed for flow control — a bounded channel automatically back-pressures producers when the consumer falls behind, which prevents unbounded memory growth that a plain concurrent queue can't do._

---

## Parallel LINQ

## [parallel-linq](parallel/parallel-linq/)

Adds `.AsParallel()` to LINQ queries to distribute work across threads.

_Learned: PLINQ parallelizes queries with one keyword, but order is not preserved by default — results arrive in thread-completion order, which is faster but will surprise you if you expected sequential order._

---

## How to Run

```bash
cd parallel/<folder-name>
dotnet run
```

The `concurrent-collections` app is a MAUI application and requires the .NET MAUI workload. All other folders are console apps.

Open `parallel/parallel.slnx` in Visual Studio or Rider to browse all projects in one solution.
