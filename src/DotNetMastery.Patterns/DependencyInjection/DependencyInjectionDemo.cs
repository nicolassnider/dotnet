namespace DotNetMastery.Patterns.DependencyInjection;

/// <summary>
/// Demonstrates the Dependency Injection (DI) pattern.
/// Compares tightly coupled instantiation (new'ing up dependencies inside the class)
/// against constructor injection using abstractions (loose coupling and testability).
/// </summary>
public static class DependencyInjectionDemo
{
    public record UserRecord(int Id, string Name, string Email);

    public interface IDatabaseConnection
    {
        UserRecord? QueryUser(int id);
    }

    public class SqlDatabaseConnection : IDatabaseConnection
    {
        private readonly Dictionary<int, UserRecord> _store = new()
        {
            [1] = new(1, "Alice", "alice@example.com"),
            [2] = new(2, "Bob", "bob@example.com")
        };

        public UserRecord? QueryUser(int id) =>
            _store.TryGetValue(id, out var user) ? user : null;
    }

    /// <summary>
    /// ❌ Anti-pattern: tightly coupled to concrete SqlDatabaseConnection.
    /// Impossible to unit test with mocks without a real database.
    /// </summary>
    public class CoupledUserService
    {
        private readonly SqlDatabaseConnection _db = new();

        public UserRecord? GetUser(int id) => _db.QueryUser(id);
    }

    /// <summary>
    /// ✅ Best Practice: Injected dependency via interface.
    /// Supports mocking, unit testing, and flexible lifetime management.
    /// </summary>
    public class DecoupledUserService
    {
        private readonly IDatabaseConnection _db;

        public DecoupledUserService(IDatabaseConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public UserRecord? GetUser(int id) => _db.QueryUser(id);
    }
}
