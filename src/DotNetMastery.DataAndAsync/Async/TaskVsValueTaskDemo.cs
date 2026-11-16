namespace DotNetMastery.DataAndAsync.Async;

/// <summary>
/// Demonstrates Task vs ValueTask performance and usage guidelines.
/// Task: Reference type on managed heap. Always allocates, even if the result is synchronous.
/// ValueTask: Value type on stack for synchronous completion. Zero allocation on hot paths.
/// Falls back to wrapping a Task only when actual asynchronous execution occurs.
/// </summary>
public static class TaskVsValueTaskDemo
{
    public record UserProfile(int Id, string Username, string Email);

    public class UserService
    {
        private readonly Dictionary<int, UserProfile> _cache = new();
        private int _networkCallCount;

        public int NetworkCallCount => _networkCallCount;

        public UserService()
        {
            // Seed local cache
            _cache[1] = new(1, "alice", "alice@example.com");
            _cache[2] = new(2, "bob", "bob@example.com");
        }

        /// <summary>
        /// ❌ Task&lt;T&gt; implementation:
        /// Even when returning immediately from cache via Task.FromResult, it allocates a Task object on the heap.
        /// </summary>
        public Task<UserProfile?> GetUserTaskAsync(int id)
        {
            if (_cache.TryGetValue(id, out var user))
            {
                return Task.FromResult<UserProfile?>(user); // Heap allocation
            }

            return FetchFromRemoteAsync(id);
        }

        /// <summary>
        /// ✅ ValueTask&lt;T&gt; implementation:
        /// When the result is already in cache, returns a stack-allocated ValueTask with ZERO heap allocation.
        /// When not in cache, transitions to asynchronous path.
        /// </summary>
        public ValueTask<UserProfile?> GetUserValueTaskAsync(int id)
        {
            if (_cache.TryGetValue(id, out var user))
            {
                // Synchronous fast path: zero heap allocation
                return new ValueTask<UserProfile?>(user);
            }

            // Asynchronous slow path
            return new ValueTask<UserProfile?>(FetchFromRemoteAsync(id));
        }

        private async Task<UserProfile?> FetchFromRemoteAsync(int id)
        {
            Interlocked.Increment(ref _networkCallCount);
            await Task.Delay(10); // Simulated network I/O
            var fetched = new UserProfile(id, $"remote_user_{id}", $"user{id}@remote.com");
            _cache[id] = fetched;
            return fetched;
        }
    }
}
