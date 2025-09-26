using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Interfaces.Services;

/// <summary>
/// Domain service interface for transaction business logic.
/// </summary>
public interface ITransactionService
{
    // Core operations
    Task<Transaction> CreateTransactionAsync(string description, Money amount, TransactionType type, int categoryId,
        DateTime? date = null);

    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task<bool> DeleteTransactionAsync(int transactionId);

    // Queries
    Task<List<Transaction>> GetAllTransactionsAsync();
    Task<List<Transaction>> GetTransactionsByPeriodAsync(DateTime startDate, DateTime endDate);

    // Statistics
    Task<Money> GetCurrentBalanceAsync();
    Task<Money> GetMonthlyIncomeAsync(int year, int month);
    Task<Money> GetMonthlyExpensesAsync(int year, int month);

    // Business validations
    Task<bool> CanDeleteTransactionAsync(int transactionId);
    Task<(bool IsValid, List<string> Errors)> IsValidTransactionAsync(Transaction transaction);
}
