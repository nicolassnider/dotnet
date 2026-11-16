using DotNetMastery.Performance.Memory;
using DotNetMastery.Performance.Span;

namespace DotNetMastery.Tests;

public class PerformanceTests
{
    [Fact]
    public void CleanSubscriber_Dispose_ShouldUnsubscribeAndPreventMemoryRetention()
    {
        var broadcaster = new MemoryLeaksDemo.EventBroadcaster();
        var subscriber = new MemoryLeaksDemo.CleanSubscriber(broadcaster);

        broadcaster.Publish("Message 1");
        Assert.Equal(1, subscriber.ReceivedCount);

        subscriber.Dispose();

        broadcaster.Publish("Message 2");
        Assert.Equal(1, subscriber.ReceivedCount); // Not incremented after Dispose
    }

    [Fact]
    public void BoundedSmartCache_ShouldEvictOldestEntry_WhenCapacityExceeded()
    {
        var cache = new MemoryLeaksDemo.BoundedSmartCache<string, int>(maxCapacity: 2);

        cache.Set("A", 1);
        Thread.Sleep(5); // Ensure distinct timestamps
        cache.Set("B", 2);
        Thread.Sleep(5);
        cache.Set("C", 3); // Exceeds capacity, should evict oldest "A"

        Assert.Equal(2, cache.Count);
        Assert.False(cache.TryGet("A", out _));
        Assert.True(cache.TryGet("B", out int valB) && valB == 2);
        Assert.True(cache.TryGet("C", out int valC) && valC == 3);
    }

    [Fact]
    public void ArrayPool_ShouldRentAndReturnBuffersSafely()
    {
        int requestedSize = 90_000; // >85KB (LOH threshold)
        int actualCapacity = MemoryLeaksDemo.ProcessLargeBufferWithPool(requestedSize);

        Assert.True(actualCapacity >= requestedSize);
    }

    [Fact]
    public void SpanParsing_ShouldYieldIdenticalResultsToSubstring_WithZeroAllocationParsing()
    {
        string rawLog = "LOG_2026-05-27_ACTIVE";

        var substringResult = SpanParsingDemo.ParseWithSubstring(rawLog);
        var spanResult = SpanParsingDemo.ParseWithSpan(rawLog);

        Assert.Equal(substringResult.Year, spanResult.Year);
        Assert.Equal(substringResult.Month, spanResult.Month);
        Assert.Equal(substringResult.Day, spanResult.Day);
        Assert.Equal(substringResult.Status, spanResult.Status);
    }

    [Fact]
    public void SpanParsing_IsActiveStatus_ShouldMatchCorrectly()
    {
        string activeLog = "LOG_2026-05-27_ACTIVE";
        string errorLog = "LOG_2026-05-27_ERROR";

        Assert.True(SpanParsingDemo.IsActiveStatusWithSpan(activeLog));
        Assert.False(SpanParsingDemo.IsActiveStatusWithSpan(errorLog));
    }
}
