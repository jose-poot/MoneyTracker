using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Infrastructure.Database;

namespace MoneyTracker.Infrastructure.Repositories;

/// Repositorio específico para categorías
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(MoneyTrackerContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        try
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting active categories: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<List<Category>> GetCategoriesWithTransactionsAsync()
    {
        try
        {
            return await _dbSet
                .Include(c => c.Transactions)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting categories with transactions: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<bool> HasTransactionsAsync(int categoryId)
    {
        try
        {
            return await _context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking if category has transactions: {ex.Message}");
            return false;
        }
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting category by name: {ex.Message}");
            return null;
        }
    }
}