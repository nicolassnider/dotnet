namespace DotNetMastery.Patterns.FactoryMethod;

/// <summary>
/// Demonstrates the Factory Method pattern.
/// Decouples client code from concrete class instantiation by providing a centralized factory.
/// </summary>
public static class FactoryMethodDemo
{
    public interface ILoggerService
    {
        string Destination { get; }
        string Log(string message);
    }

    public class FileLoggerService : ILoggerService
    {
        public string Destination => "File";
        public string Log(string message) => $"[FILE] {message}";
    }

    public class DatabaseLoggerService : ILoggerService
    {
        public string Destination => "Database";
        public string Log(string message) => $"[DB] {message}";
    }

    public class ConsoleLoggerService : ILoggerService
    {
        public string Destination => "Console";
        public string Log(string message) => $"[CONSOLE] {message}";
    }

    /// <summary>
    /// Factory providing instances based on configured type or runtime parameter.
    /// Uses modern C# switch expression.
    /// </summary>
    public static class LoggerFactory
    {
        public static ILoggerService CreateLogger(string type) => type?.Trim().ToLowerInvariant() switch
        {
            "file" => new FileLoggerService(),
            "db" or "database" => new DatabaseLoggerService(),
            _ => new ConsoleLoggerService()
        };
    }
}
