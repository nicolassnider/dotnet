using System.Buffers;
using Microsoft.Extensions.Caching.Memory;

namespace DotNetMastery.Performance.Memory;

/// <summary>
/// Demonstrates common Memory Leak scenarios in .NET and their modern solutions:
/// 1. Event subscription leaks (unsubscribed events retaining references)
/// 2. Unbounded static caches vs bounded caches with eviction
/// 3. MemoryCache with expiration policies
/// 4. Large Object Heap (LOH) pressure (>85KB) vs ArrayPool&lt;byte&gt;
/// 5. Task closure retention
/// </summary>
public static class MemoryLeaksDemo
{
    // ==========================================
    // 1. Event Subscription Leaks
    // ==========================================

    public class EventBroadcaster
    {
        public event Action<string>? OnDataReceived;

        public void Publish(string data) => OnDataReceived?.Invoke(data);
    }

    /// <summary>
    /// ❌ Leaky Subscriber: Never unsubscribes from the publisher.
    /// As long as EventBroadcaster lives, this subscriber cannot be collected by GC.
    /// </summary>
    public class LeakySubscriber
    {
        public int ReceivedCount { get; private set; }

        public LeakySubscriber(EventBroadcaster broadcaster)
        {
            broadcaster.OnDataReceived += HandleData;
        }

        private void HandleData(string data) => ReceivedCount++;
    }

    /// <summary>
    /// ✅ Clean Subscriber: Implements IDisposable and explicitly detaches the delegate,
    /// allowing GC to reclaim the object.
    /// </summary>
    public class CleanSubscriber : IDisposable
    {
        private readonly EventBroadcaster _broadcaster;
        public int ReceivedCount { get; private set; }

        public CleanSubscriber(EventBroadcaster broadcaster)
        {
            _broadcaster = broadcaster;
            _broadcaster.OnDataReceived += HandleData;
        }

        private void HandleData(string data) => ReceivedCount++;

        public void Dispose()
        {
            _broadcaster.OnDataReceived -= HandleData;
        }
    }

    // ==========================================
    // 2. Unbounded Static Cache vs Bounded Smart Cache
    // ==========================================

    /// <summary>
    /// ✅ Bounded Cache with eviction policy.
    /// Prevents memory leaks by maintaining a strict maximum capacity and evicting oldest entries.
    /// </summary>
    public class BoundedSmartCache<TKey, TValue> where TKey : notnull
    {
        private readonly int _maxCapacity;
        private readonly Dictionary<TKey, CacheItem> _store = new();
        private readonly object _lock = new();

        public record CacheItem(TValue Value, DateTime CreatedAtUtc);

        public BoundedSmartCache(int maxCapacity = 100)
        {
            if (maxCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Capacity must be positive.");

            _maxCapacity = maxCapacity;
        }

        public int Count
        {
            get
            {
                lock (_lock) return _store.Count;
            }
        }

        public void Set(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_store.Count >= _maxCapacity && !_store.ContainsKey(key))
                {
                    // Evict oldest entry (FIFO / LRU basis)
                    var oldest = _store.OrderBy(kv => kv.Value.CreatedAtUtc).First();
                    _store.Remove(oldest.Key);
                }

                _store[key] = new CacheItem(value, DateTime.UtcNow);
            }
        }

        public bool TryGet(TKey key, out TValue? value)
        {
            lock (_lock)
            {
                if (_store.TryGetValue(key, out var item))
                {
                    value = item.Value;
                    return true;
                }

                value = default;
                return false;
            }
        }
    }

    // ==========================================
    // 3. IMemoryCache with Expiration
    // ==========================================

    public class ExpiringDataService
    {
        private readonly IMemoryCache _memoryCache;

        public ExpiringDataService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public string GetData(string key)
        {
            return _memoryCache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                return $"Computed_Value_For_{key}";
            })!;
        }
    }

    // ==========================================
    // 4. Large Object Heap (LOH) & ArrayPool
    // ==========================================

    /// <summary>
    /// Objects >= 85,000 bytes are placed on the Large Object Heap (LOH).
    /// Repeated allocation and garbage collection of large buffers causes severe heap fragmentation.
    /// ArrayPool&lt;byte&gt;.Shared rents and returns buffers without continuously stressing the LOH.
    /// </summary>
    public static int ProcessLargeBufferWithPool(int requiredBytes)
    {
        // Rent from shared pool
        byte[] buffer = ArrayPool<byte>.Shared.Rent(requiredBytes);

        try
        {
            // Simulate work on the rented buffer
            buffer[0] = 0xAA;
            buffer[requiredBytes - 1] = 0xBB;
            return buffer.Length;
        }
        finally
        {
            // Always return rented buffer in finally block
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // ==========================================
    // 5. Task Closure Retention
    // ==========================================

    /// <summary>
    /// Demonstrates capturing a scalar instead of the entire large array in a task closure,
    /// allowing GC to collect the large buffer immediately.
    /// </summary>
    public static async Task<int> ProcessWithMinimalClosureAsync()
    {
        var largeBuffer = new byte[100_000];
        largeBuffer[0] = 42;

        // Extract scalar so the closure does NOT capture largeBuffer
        int initialValue = largeBuffer[0];
        largeBuffer = null; // Eligible for GC collection

        return await Task.Run(async () =>
        {
            await Task.Delay(5);
            return initialValue * 2;
        });
    }
}
