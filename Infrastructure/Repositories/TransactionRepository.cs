using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Core.ValueObjects;
using MoneyTracker.Infrastructure.Database;

namespace MoneyTracker.Infrastructure.Repositories;

/// <summary>
/// Repository specialized for transactions with optimized queries.
/// </summary>
public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(MoneyTrackerContext context) : base(context)
    {
    }

    /// <summary>
    /// Override to automatically include Category.
    /// </summary>
    public override async Task<List<Transaction>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public override async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting transactions by date range: {ex.Message}");
            return new List<Transaction>();
        }
    }

    public async Task<List<Transaction>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            return await _dbSet
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting transactions by category: {ex.Message}");
            return new List<Transaction>();
        }
    }

    public async Task<List<Transaction>> GetByTypeAsync(TransactionType type)
    {
        try
        {
            return await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting transactions by type: {ex.Message}");
            return new List<Transaction>();
        }
    }

    public async Task<Money> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _dbSet.Where(t => t.Type == TransactionType.Income);

            if (startDate.HasValue)
                query = query.Where(t => t.Date.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(t => t.Date.Date <= endDate.Value.Date);

            var total = await query.SumAsync(t => t.AmountValue);
            return new Money(Math.Abs(total), "USD");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating total income: {ex.Message}");
            return new Money(0, "USD");
        }
    }

    public async Task<Money> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _dbSet.Where(t => t.Type == TransactionType.Expense);

            if (startDate.HasValue)
                query = query.Where(t => t.Date.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(t => t.Date.Date <= endDate.Value.Date);

            var total = await query.SumAsync(t => t.AmountValue);
            return new Money(Math.Abs(total), "USD");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating total expenses: {ex.Message}");
            return new Money(0, "USD");
        }
    }

    public async Task<Money> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var income = await GetTotalIncomeAsync(startDate, endDate);
            var expenses = await GetTotalExpensesAsync(startDate, endDate);

            return income - expenses;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating balance: {ex.Message}");
            return new Money(0, "USD");
        }
    }

    public async Task<List<Transaction>> GetTransactionsThisMonthAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await GetByDateRangeAsync(startOfMonth, endOfMonth);
    }

    public async Task<List<Transaction>> GetTransactionsThisYearAsync()
    {
        var now = DateTime.Now;
        var startOfYear = new DateTime(now.Year, 1, 1);
        var endOfYear = new DateTime(now.Year, 12, 31);

        return await GetByDateRangeAsync(startOfYear, endOfYear);
    }

    public async Task<List<(Category Category, Money Total)>> GetTopCategoriesAsync(int count = 5)
    {
        try
        {
            var result = await _dbSet
                .Include(t => t.Category)
                .Where(t => t.Type == TransactionType.Expense) // Only expenses for the "top" calculation
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => Math.Abs(t.AmountValue))
                })
                .OrderByDescending(x => x.Total)
                .Take(count)
                .ToListAsync();

            return result
                .Select(x => (x.Category, new Money(x.Total, "USD")))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting top categories: {ex.Message}");
            return new List<(Category, Money)>();
        }
    }

    public async Task<List<Transaction>> GetRecentTransactionsAsync(int count = 10)
    {
        try
        {
            return await _dbSet
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting recent transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }
}