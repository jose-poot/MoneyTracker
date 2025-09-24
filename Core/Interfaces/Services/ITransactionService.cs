using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Interfaces.Services;

/// <summary>
/// Servicio de dominio para lógica de negocio de transacciones
/// </summary>
public interface ITransactionService
{
    // Operaciones principales
    Task<Transaction> CreateTransactionAsync(string description, Money amount, TransactionType type, int categoryId,
        DateTime? date = null);

    Task<Transaction> UpdateTransactionAsync(Transaction transaction);
    Task<bool> DeleteTransactionAsync(int transactionId);

    // Consultas
    Task<List<Transaction>> GetAllTransactionsAsync();
    Task<List<Transaction>> GetTransactionsByPeriodAsync(DateTime startDate, DateTime endDate);

    // Estadísticas
    Task<Money> GetCurrentBalanceAsync();
    Task<Money> GetMonthlyIncomeAsync(int year, int month);
    Task<Money> GetMonthlyExpensesAsync(int year, int month);

    // Validaciones de negocio
    Task<bool> CanDeleteTransactionAsync(int transactionId);
    Task<(bool IsValid, List<string> Errors)> IsValidTransactionAsync(Transaction transaction);
}
