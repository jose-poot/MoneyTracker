using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface specialized for transactions with additional queries.
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    // Transaction-specific queries
    Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Transaction>> GetByCategoryAsync(int categoryId);
    Task<List<Transaction>> GetByTypeAsync(TransactionType type);

    // Statistics and aggregations
    Task<Money> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Money> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Money> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Period-based queries
    Task<List<Transaction>> GetTransactionsThisMonthAsync();
    Task<List<Transaction>> GetTransactionsThisYearAsync();

    // Top categories
    Task<List<(Category Category, Money Total)>> GetTopCategoriesAsync(int count = 5);

    // Recent transactions
    Task<List<Transaction>> GetRecentTransactionsAsync(int count = 10);
}