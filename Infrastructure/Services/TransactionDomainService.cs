using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Core.Interfaces.Services;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de dominio para transacciones
/// </summary>
public class TransactionDomainService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionDomainService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Transaction> CreateTransactionAsync(
        string description,
        Money amount,
        TransactionType type,
        int categoryId,
        DateTime? date = null)
    {
        // 1. Validar que la categoría existe y está activa
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new ArgumentException("La categoría no existe", nameof(categoryId));

        if (!category.IsActive)
            throw new ArgumentException("La categoría está inactiva", nameof(categoryId));

        // 2. Crear transacción usando factory methods del dominio
        var transaction = type == TransactionType.Expense
            ? Transaction.CreateExpense(description, amount, categoryId, date)
            : Transaction.CreateIncome(description, amount, categoryId, date);

        // 3. Validar reglas de dominio
        if (!transaction.IsValid(out var errors))
        {
            throw new ArgumentException($"Transacción inválida: {string.Join(", ", errors)}");
        }

        // 4. Guardar
        return await _transactionRepository.AddAsync(transaction);
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        // 1. Validar que existe
        var existing = await _transactionRepository.GetByIdAsync(transaction.Id);
        if (existing == null)
            throw new ArgumentException("La transacción no existe");

        // 2. Validar reglas de dominio
        if (!transaction.IsValid(out var errors))
        {
            throw new ArgumentException($"Transacción inválida: {string.Join(", ", errors)}");
        }

        // 3. Actualizar
        transaction.MarkAsUpdated();
        return await _transactionRepository.UpdateAsync(transaction);
    }

    public async Task<bool> DeleteTransactionAsync(int transactionId)
    {
        return await _transactionRepository.DeleteAsync(transactionId);
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        return await _transactionRepository.GetAllAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<Money> GetCurrentBalanceAsync()
    {
        return await _transactionRepository.GetBalanceAsync();
    }

    public async Task<Money> GetMonthlyIncomeAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _transactionRepository.GetTotalIncomeAsync(startDate, endDate);
    }

    public async Task<Money> GetMonthlyExpensesAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _transactionRepository.GetTotalExpensesAsync(startDate, endDate);
    }

    public async Task<bool> CanDeleteTransactionAsync(int transactionId)
    {
        // Por ahora, todas las transacciones se pueden eliminar
        // En el futuro podrían haber restricciones (ej: transacciones auditadas)
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        return transaction != null;
    }

    public async Task<(bool IsValid, List<string> Errors)> IsValidTransactionAsync(Transaction transaction)
    {
        var errors = new List<string>();

        // Validaciones del dominio
        if (!transaction.IsValid(out var domainErrors))
        {
            errors.AddRange(domainErrors);
        }

        // Validaciones que requieren repositorio
        var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
        if (category == null)
            errors.Add("La categoría no existe");
        else if (!category.IsActive)
            errors.Add("La categoría está inactiva");

        return (errors.Count == 0, errors);
    }
}