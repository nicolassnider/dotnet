using Microsoft.EntityFrameworkCore;

namespace DotNetMastery.Patterns.Repository;

/// <summary>
/// Demonstrates the Repository Pattern.
/// Encapsulates data access logic and abstracts the underlying ORM/database,
/// exposing a clean, collection-like interface to the domain/service layer.
/// </summary>
public static class RepositoryPatternDemo
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users => Set<UserEntity>();
    }

    public interface IUserRepository
    {
        Task<UserEntity?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<UserEntity>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(UserEntity user, CancellationToken ct = default);
        Task UpdateAsync(UserEntity user, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UserEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<List<UserEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Users.ToListAsync(ct);
        }

        public async Task AddAsync(UserEntity user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(UserEntity user, CancellationToken ct = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await GetByIdAsync(id, ct);
            if (existing != null)
            {
                _context.Users.Remove(existing);
                await _context.SaveChangesAsync(ct);
            }
        }
    }

    public class UserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public Task<UserEntity?> FetchUserAsync(int id, CancellationToken ct = default) =>
            _repository.GetByIdAsync(id, ct);

        public Task RegisterUserAsync(UserEntity user, CancellationToken ct = default) =>
            _repository.AddAsync(user, ct);
    }
}
