using Microsoft.EntityFrameworkCore;

namespace DotNetMastery.DataAndAsync.Collections;

/// <summary>
/// Demonstrates collection interfaces and LINQ query execution:
/// 1. IEnumerable vs IQueryable vs List
/// 2. OrderBy vs ThenBy chaining
/// 3. GroupBy vs modern CountBy (.NET 9+)
/// </summary>
public static class CollectionsAndLinqDemo
{
    public record Product(int Id, string Category, string Name, decimal Price, int Stock);

    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
    }

    // ==========================================
    // 1. IEnumerable vs IQueryable vs List
    // ==========================================

    /// <summary>
    /// Demonstrates IQueryable: the filter condition is part of the expression tree
    /// and is translated to SQL/data-source filtering before items are retrieved.
    /// </summary>
    public static async Task<List<Product>> QueryWithIQueryableAsync(StoreDbContext context, decimal minPrice)
    {
        // IQueryable - filtered at the data source level
        IQueryable<Product> query = context.Products.Where(p => p.Price >= minPrice);
        return await query.ToListAsync();
    }

    /// <summary>
    /// Demonstrates IEnumerable: fetches all items into memory first, then applies filtering in memory.
    /// Less efficient on large remote datasets.
    /// </summary>
    public static List<Product> QueryWithIEnumerable(StoreDbContext context, decimal minPrice)
    {
        // AsEnumerable() pulls all rows from database into memory first
        IEnumerable<Product> inMemoryStream = context.Products.AsEnumerable();
        return inMemoryStream.Where(p => p.Price >= minPrice).ToList();
    }

    // ==========================================
    // 2. OrderBy vs ThenBy
    // ==========================================

    /// <summary>
    /// ❌ Flawed ordering: subsequent OrderBy calls discard previous sorting criteria.
    /// </summary>
    public static List<Product> OrderByFlawed(IEnumerable<Product> products)
    {
        return products
            .OrderBy(p => p.Category)
            .OrderBy(p => p.Price) // ⚠️ Resets and overwrites Category ordering!
            .ToList();
    }

    /// <summary>
    /// ✅ Correct multi-column ordering: OrderBy for primary, ThenBy for secondary criteria.
    /// </summary>
    public static List<Product> OrderByCorrect(IEnumerable<Product> products)
    {
        return products
            .OrderBy(p => p.Category) // Primary key
            .ThenBy(p => p.Price)     // Secondary key
            .ThenBy(p => p.Name)      // Tertiary key
            .ToList();
    }

    // ==========================================
    // 3. GroupBy vs CountBy (.NET 9+)
    // ==========================================

    /// <summary>
    /// Legacy GroupBy: allocates intermediate grouping collections in memory even when only count is needed.
    /// </summary>
    public static Dictionary<string, int> CountByCategoryLegacy(IEnumerable<Product> products)
    {
        return products
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Modern CountBy (.NET 9+): single-pass frequency count without materializing intermediate group collections.
    /// Significant allocation and CPU savings.
    /// </summary>
    public static Dictionary<string, int> CountByCategoryModern(IEnumerable<Product> products)
    {
        return products
            .CountBy(p => p.Category)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
