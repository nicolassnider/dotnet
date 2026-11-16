namespace DotNetMastery.DataAndAsync.Async;

/// <summary>
/// Implements and demonstrates the 11 Essential Rules of Async/Await in .NET.
/// </summary>
public static class AsyncRulesDemo
{
    // ==========================================
    // Rule 1: Never block async code (.Result / .Wait())
    // ==========================================

    /// <summary>
    /// ✅ Rule 1: Always await tasks asynchronously rather than using .Result or .Wait(),
    /// which blocks the calling thread and risks synchronization context deadlocks.
    /// </summary>
    public static async Task<string> Rule1_FetchDataNonBlockingAsync()
    {
        await Task.Delay(10);
        return "Async data fetched cleanly";
    }

    // ==========================================
    // Rule 2: Never return async void for tasks (Task only)
    // ==========================================

    /// <summary>
    /// ✅ Rule 2: Methods returning Task can be awaited, observed, and their exceptions handled.
    /// Async void should only be used for UI event handlers.
    /// </summary>
    public static async Task Rule2_ProcessTaskAsync()
    {
        await Task.Delay(10);
    }

    // ==========================================
    // Rule 3: Execute independent tasks in parallel with Task.WhenAll
    // ==========================================

    /// <summary>
    /// ✅ Rule 3: Start independent async operations concurrently and await them together.
    /// </summary>
    public static async Task<(int first, string second)> Rule3_RunInParallelAsync()
    {
        Task<int> taskA = FetchNumberAsync(10);
        Task<string> taskB = FetchTextAsync("Sample");

        await Task.WhenAll(taskA, taskB);

        return (await taskA, await taskB);
    }

    // ==========================================
    // Rule 4: Do not wrap async I/O in Task.Run
    // ==========================================

    /// <summary>
    /// ✅ Rule 4: Asynchronous I/O operations already yield thread execution via completion ports.
    /// Wrapping them in Task.Run wastes a thread-pool thread just to await another thread.
    /// </summary>
    public static async Task<string> Rule4_DirectAsyncIoAsync()
    {
        // Direct non-blocking call without Task.Run overhead
        await Task.Delay(10);
        return "I/O Completed";
    }

    // ==========================================
    // Rule 5: Use ConfigureAwait(false) in library code
    // ==========================================

    /// <summary>
    /// ✅ Rule 5: In reusable non-UI library code, ConfigureAwait(false) avoids
    /// capturing the current SynchronizationContext, yielding higher throughput and preventing deadlocks.
    /// </summary>
    public static async Task<int> Rule5_LibraryMethodWithConfigureAwaitAsync()
    {
        await Task.Delay(10).ConfigureAwait(false);
        return 42;
    }

    // ==========================================
    // Rule 6: Keep async all the way up the stack
    // ==========================================

    /// <summary>
    /// ✅ Rule 6: Async should propagate end-to-end (controller -> service -> repository).
    /// </summary>
    public static async Task<string> Rule6_AsyncAllTheWayAsync()
    {
        return await StepTwoAsync();

        static async Task<string> StepTwoAsync() => await StepThreeAsync();
        static async Task<string> StepThreeAsync()
        {
            await Task.Delay(5);
            return "Propagated successfully";
        }
    }

    // ==========================================
    // Rule 7: Always observe and handle task exceptions
    // ==========================================

    /// <summary>
    /// ✅ Rule 7: Awaiting a faulted task unwraps the AggregateException to the first original exception,
    /// enabling clear and predictable error handling.
    /// </summary>
    public static async Task<bool> Rule7_HandleTaskExceptionAsync()
    {
        try
        {
            await FaultyOperationAsync();
            return false;
        }
        catch (InvalidOperationException)
        {
            // Successfully caught and handled
            return true;
        }

        static async Task FaultyOperationAsync()
        {
            await Task.Delay(5);
            throw new InvalidOperationException("Simulated fault");
        }
    }

    // ==========================================
    // Rule 8: Do not mark synchronous methods as async
    // ==========================================

    /// <summary>
    /// ✅ Rule 8: If a method does not await anything, do not make it async.
    /// Return the value directly or Task.FromResult/CompletedTask to avoid state machine compilation.
    /// </summary>
    public static Task<string> Rule8_SynchronousFastPath(string cachedValue)
    {
        return Task.FromResult(cachedValue);
    }

    // ==========================================
    // Rule 9: Pass CancellationToken to long-running operations
    // ==========================================

    /// <summary>
    /// ✅ Rule 9: Propagate CancellationToken to allow graceful cancellation and responsive resource cleanup.
    /// </summary>
    public static async Task<bool> Rule9_CancellableWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(500, cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    // ==========================================
    // Rule 10: Avoid sequential await in loops for independent operations
    // ==========================================

    /// <summary>
    /// ✅ Rule 10: Instead of awaiting each iteration sequentially, launch all tasks concurrently
    /// and await with Task.WhenAll.
    /// </summary>
    public static async Task<List<int>> Rule10_ParallelLoopAsync(IEnumerable<int> ids)
    {
        var tasks = ids.Select(FetchNumberAsync);
        int[] results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    // ==========================================
    // Rule 11: Stream results concurrently with Task.WhenEach (.NET 9+)
    // ==========================================

    /// <summary>
    /// ✅ Rule 11: Task.WhenEach yields completed tasks as they finish, without waiting
    /// for all other tasks or maintaining completion order.
    /// </summary>
    public static async Task<List<string>> Rule11_StreamWithWhenEachAsync(IEnumerable<Task<string>> tasks)
    {
        var completedResults = new List<string>();

        await foreach (Task<string> finishedTask in Task.WhenEach(tasks))
        {
            completedResults.Add(await finishedTask);
        }

        return completedResults;
    }

    // Internal helper methods
    private static async Task<int> FetchNumberAsync(int value)
    {
        await Task.Delay(5);
        return value * 2;
    }

    private static async Task<string> FetchTextAsync(string value)
    {
        await Task.Delay(5);
        return $"Processed_{value}";
    }
}
