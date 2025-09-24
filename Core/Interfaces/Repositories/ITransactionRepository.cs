using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Interfaces.Repositories;

/// <summary>
/// Repositorio específico para transacciones con métodos especializados
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    // Consultas específicas de transacciones
    Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Transaction>> GetByCategoryAsync(int categoryId);
    Task<List<Transaction>> GetByTypeAsync(TransactionType type);

    // Estadísticas y agregaciones
    Task<Money> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Money> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Money> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Consultas por período
    Task<List<Transaction>> GetTransactionsThisMonthAsync();
    Task<List<Transaction>> GetTransactionsThisYearAsync();

    // Top categorías
    Task<List<(Category Category, Money Total)>> GetTopCategoriesAsync(int count = 5);

    // Transacciones recientes
    Task<List<Transaction>> GetRecentTransactionsAsync(int count = 10);
}