using MoneyTracker.Core.Entities;

namespace MoneyTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface specialized for categories.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetActiveCategoriesAsync();
    Task<List<Category>> GetCategoriesWithTransactionsAsync();
    Task<bool> HasTransactionsAsync(int categoryId);
    Task<Category?> GetByNameAsync(string name);
}