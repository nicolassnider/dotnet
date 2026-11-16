using DotNetMastery.DataAndAsync.Async;
using DotNetMastery.DataAndAsync.Collections;

namespace DotNetMastery.Tests;

public class DataAndAsyncTests
{
    private readonly List<CollectionsAndLinqDemo.Product> _sampleProducts =
    [
        new(1, "Hardware", "Keyboard", 75m, 10),
        new(2, "Hardware", "Monitor", 300m, 5),
        new(3, "Software", "IDE License", 150m, 25),
        new(4, "Software", "Cloud Credits", 50m, 100),
        new(5, "Hardware", "Mouse", 25m, 50)
    ];

    [Fact]
    public void Linq_OrderByThenBy_ShouldSortByMultipleKeysCorrectly()
    {
        var sorted = CollectionsAndLinqDemo.OrderByCorrect(_sampleProducts);

        // Hardware should precede Software
        Assert.Equal("Hardware", sorted[0].Category);
        // Within Hardware, prices should be ascending: 25 -> 75 -> 300
        Assert.Equal(25m, sorted[0].Price);
        Assert.Equal(75m, sorted[1].Price);
        Assert.Equal(300m, sorted[2].Price);
    }

    [Fact]
    public void Linq_CountBy_ShouldCountOccurrencesAccurately()
    {
        var counts = CollectionsAndLinqDemo.CountByCategoryModern(_sampleProducts);

        Assert.Equal(3, counts["Hardware"]);
        Assert.Equal(2, counts["Software"]);
    }

    [Fact]
    public async Task AsyncRules_Rule1_ShouldFetchNonBlocking()
    {
        string result = await AsyncRulesDemo.Rule1_FetchDataNonBlockingAsync();
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task AsyncRules_Rule3_ShouldExecuteInParallel()
    {
        var (first, second) = await AsyncRulesDemo.Rule3_RunInParallelAsync();
        Assert.Equal(20, first);
        Assert.Equal("Processed_Sample", second);
    }

    [Fact]
    public async Task AsyncRules_Rule7_ShouldHandleExceptionGracefully()
    {
        bool handled = await AsyncRulesDemo.Rule7_HandleTaskExceptionAsync();
        Assert.True(handled);
    }

    [Fact]
    public async Task AsyncRules_Rule9_ShouldCancelGracefully()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancelled

        bool completed = await AsyncRulesDemo.Rule9_CancellableWorkAsync(cts.Token);
        Assert.False(completed);
    }

    [Fact]
    public async Task AsyncRules_Rule11_TaskWhenEach_ShouldYieldCompletedTasks()
    {
        var tasks = new[]
        {
            Task.Run(async () => { await Task.Delay(10); return "Fast"; }),
            Task.Run(async () => { await Task.Delay(30); return "Slow"; })
        };

        var streamed = await AsyncRulesDemo.Rule11_StreamWithWhenEachAsync(tasks);

        Assert.Equal(2, streamed.Count);
        Assert.Contains("Fast", streamed);
        Assert.Contains("Slow", streamed);
    }

    [Fact]
    public async Task TaskVsValueTask_ShouldAvoidRemoteCallWhenCached()
    {
        var service = new TaskVsValueTaskDemo.UserService();

        // User 1 is seeded in cache
        var userCached = await service.GetUserValueTaskAsync(1);
        Assert.NotNull(userCached);
        Assert.Equal("alice", userCached.Username);
        Assert.Equal(0, service.NetworkCallCount);

        // User 99 is not in cache, will trigger network call
        var userRemote = await service.GetUserValueTaskAsync(99);
        Assert.NotNull(userRemote);
        Assert.Equal(1, service.NetworkCallCount);
    }
}
