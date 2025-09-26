using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Core.Interfaces.Services;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Infrastructure.Services;

/// <summary>
/// Domain service implementation for transactions.
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
        // 1. Validate that the category exists and is active
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new ArgumentException("The category does not exist", nameof(categoryId));

        if (!category.IsActive)
            throw new ArgumentException("The category is inactive", nameof(categoryId));

        // 2. Create the transaction using domain factory methods
        var transaction = type == TransactionType.Expense
            ? Transaction.CreateExpense(description, amount, categoryId, date)
            : Transaction.CreateIncome(description, amount, categoryId, date);

        // 3. Validate domain rules
        if (!transaction.IsValid(out var errors))
        {
            throw new ArgumentException($"Invalid transaction: {string.Join(", ", errors)}");
        }

        // 4. Persist the transaction
        return await _transactionRepository.AddAsync(transaction);
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        // 1. Ensure the transaction exists
        var existing = await _transactionRepository.GetByIdAsync(transaction.Id);
        if (existing == null)
            throw new ArgumentException("The transaction does not exist");

        // 2. Validate domain rules
        if (!transaction.IsValid(out var errors))
        {
            throw new ArgumentException($"Invalid transaction: {string.Join(", ", errors)}");
        }

        // 3. Apply the update
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
        // For now all transactions can be deleted
        // Future restrictions may be added (for example: audited transactions)
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        return transaction != null;
    }

    public async Task<(bool IsValid, List<string> Errors)> IsValidTransactionAsync(Transaction transaction)
    {
        var errors = new List<string>();

        // Domain-level validations
        if (!transaction.IsValid(out var domainErrors))
        {
            errors.AddRange(domainErrors);
        }

        // Repository-dependent validations
        var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
        if (category == null)
            errors.Add("The category does not exist");
        else if (!category.IsActive)
            errors.Add("The category is inactive");

        return (errors.Count == 0, errors);
    }
}