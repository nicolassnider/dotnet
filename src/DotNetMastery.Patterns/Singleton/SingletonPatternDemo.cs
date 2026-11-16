namespace DotNetMastery.Patterns.Singleton;

/// <summary>
/// Demonstrates the Singleton Pattern.
/// Ensures a class has only one instance and provides a global point of access to it.
/// Compares naive implementation vs thread-safe Lazy&lt;T&gt; vs .NET DI AddSingleton.
/// </summary>
public static class SingletonPatternDemo
{
    /// <summary>
    /// ❌ Naive Singleton: Not thread-safe. Multiple threads can enter simultaneously
    /// and create multiple instances.
    /// </summary>
    public sealed class NaiveAppSettings
    {
        private static NaiveAppSettings? _instance;

        public string AppName { get; set; } = "DotNetMastery";

        private NaiveAppSettings() { }

        public static NaiveAppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NaiveAppSettings();
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// ✅ Thread-safe, lazy-initialized Singleton using Lazy&lt;T&gt;.
    /// Guarantees thread-safe single instantiation only upon first access.
    /// </summary>
    public sealed class LazyAppSettings
    {
        private static readonly Lazy<LazyAppSettings> _lazyInstance =
            new(() => new LazyAppSettings());

        public string ApiVersion { get; } = "2.0";
        public DateTime InitializedAtUtc { get; } = DateTime.UtcNow;

        private LazyAppSettings() { }

        public static LazyAppSettings Instance => _lazyInstance.Value;
    }

    /// <summary>
    /// ✅ Recommended in modern .NET: DI-managed Singleton.
    /// Injected via IServiceCollection.AddSingleton&lt;AppConfiguration&gt;(),
    /// which allows lifecycle control, interface abstraction, and testability.
    /// </summary>
    public class AppConfiguration
    {
        public string Environment { get; set; } = "Production";
        public int CacheTimeoutSeconds { get; set; } = 300;
    }
}
