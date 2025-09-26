using AutoMapper;
using FluentValidation;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Validators;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Enums;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Core.Interfaces.Services;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Application.Services;
/// <summary>
/// Application service for transactions.
/// Coordina validaciones, conversiones y llamadas a repositorios
/// </summary>
public class TransactionAppService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionService _transactionDomainService;
    private readonly IMapper _mapper;
    private readonly CreateTransactionValidator _createValidator;
    private readonly IValidator<TransactionDto> _updateValidator;

    public TransactionAppService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ITransactionService transactionDomainService,
        IMapper mapper,
        CreateTransactionValidator createValidator,
        IValidator<TransactionDto> updateValidator)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _transactionDomainService = transactionDomainService;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Retrieves every transaction.
    /// </summary>
    public async Task<List<TransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return _mapper.Map<List<TransactionDto>>(transactions);
    }

    /// <summary>
    /// Retrieves transactions by date range.
    /// </summary>
    public async Task<List<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<List<TransactionDto>>(transactions);
    }

    /// <summary>
    /// Retrieves a specific transaction.
    /// </summary>
    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
    }

    /// <summary>
    /// Creates a new transaction with full validation.
    /// </summary>
    public async Task<(bool Success, TransactionDto? Transaction, List<string> Errors)> CreateTransactionAsync(CreateTransactionDto createDto)
    {
        // 1. Validate the input data
        var validationResult = await _createValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Convert DTO to an entity
            var money = new Money(createDto.Amount, createDto.Currency);

            // 3. Use domain factory methods to create the transaction properly
            var transaction = createDto.Type == TransactionType.Expense
                ? Transaction.CreateExpense(createDto.Description, money, createDto.CategoryId, createDto.Date)
                : Transaction.CreateIncome(createDto.Description, money, createDto.CategoryId, createDto.Date);

            // 4. Set additional properties
            transaction.Notes = createDto.Notes;
            transaction.Location = createDto.Location;
            transaction.IsRecurring = createDto.IsRecurring;

            // 5. Validate domain rules
            if (!transaction.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 6. Persist to the repository
            var savedTransaction = await _transactionRepository.AddAsync(transaction);

            // 7. Map back to a DTO for the response
            var resultDto = _mapper.Map<TransactionDto>(savedTransaction);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            // 8. Error handling
            var errorMessage = "Error creating the transaction: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    public async Task<(bool Success, TransactionDto? Transaction, List<string> Errors)> UpdateTransactionAsync(TransactionDto transactionDto)
    {
        // 1. Validate input data
        var validationResult = await _updateValidator.ValidateAsync(transactionDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Retrieve existing transaction
            var existingTransaction = await _transactionRepository.GetByIdAsync(transactionDto.Id);
            if (existingTransaction == null)
            {
                return (false, null, new List<string> { "The transaction does not exist" });
            }

            // 3. Update mutable properties
            existingTransaction.Description = transactionDto.Description;
            existingTransaction.Amount = new Money(transactionDto.Amount, transactionDto.Currency);
            existingTransaction.CategoryId = transactionDto.CategoryId;
            existingTransaction.Date = transactionDto.Date;
            existingTransaction.Notes = transactionDto.Notes;
            existingTransaction.Location = transactionDto.Location;
            existingTransaction.IsRecurring = transactionDto.IsRecurring;

            // 4. Change the type if necessary
            if (existingTransaction.Type != transactionDto.Type)
            {
                existingTransaction.ChangeType(transactionDto.Type);
            }

            existingTransaction.MarkAsUpdated();

            // 5. Validate domain rules
            if (!existingTransaction.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 6. Persist the changes in the repository
            var updatedTransaction = await _transactionRepository.UpdateAsync(existingTransaction);

            // 7. Map back to a DTO
            var resultDto = _mapper.Map<TransactionDto>(updatedTransaction);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            var errorMessage = "Error updating the transaction: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    public async Task<(bool Success, List<string> Errors)> DeleteTransactionAsync(int id)
    {
        try
        {
            // 1. Ensure it exists
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return (false, new List<string> { "The transaction does not exist" });
            }

            // 2. Verify business rules (additional validations can be added)
            var canDelete = await _transactionDomainService.CanDeleteTransactionAsync(id);
            if (!canDelete)
            {
                return (false, new List<string> { "This transaction cannot be deleted" });
            }

            // 3. Delete
            var deleted = await _transactionRepository.DeleteAsync(id);

            return deleted
                ? (true, new List<string>())
                : (false, new List<string> { "Error deleting the transaction" });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error deleting the transaction: " + ex.Message;
            return (false, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Retrieves the financial summary for the current month.
    /// </summary>
    public async Task<SummaryDto> GetMonthlySummaryAsync(int? year = null, int? month = null)
    {
        var targetDate = new DateTime(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, 1);
        var startDate = targetDate;
        var endDate = startDate.AddMonths(1).AddDays(-1);

        try
        {
            // 1. Retrieve transactions for the period
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // 2. Calculate totals using the domain service
            var balance = await _transactionDomainService.GetCurrentBalanceAsync();
            var monthlyIncome = await _transactionDomainService.GetMonthlyIncomeAsync(targetDate.Year, targetDate.Month);
            var monthlyExpenses = await _transactionDomainService.GetMonthlyExpensesAsync(targetDate.Year, targetDate.Month);

            // 3. Retrieve the most used categories
            var topCategories = await _transactionRepository.GetTopCategoriesAsync(5);

            // 4. Retrieve recent transactions
            var recentTransactions = await _transactionRepository.GetRecentTransactionsAsync(5);

            // 5. Build the summary DTO
            var summary = new SummaryDto
            {
                CurrentBalance = balance.Amount,
                MonthlyIncome = monthlyIncome.Amount,
                MonthlyExpenses = Math.Abs(monthlyExpenses.Amount), // Display as a positive value
                MonthlyBalance = monthlyIncome.Amount - Math.Abs(monthlyExpenses.Amount),
                Currency = balance.Currency,
                TotalTransactionsThisMonth = transactions.Count,
                TotalCategoriesUsed = transactions.Select(t => t.CategoryId).Distinct().Count(),
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TopExpenseCategories = _mapper.Map<List<CategorySummaryDto>>(topCategories.Where(c => c.Total.Amount > 0)),
                RecentTransactions = _mapper.Map<List<TransactionDto>>(recentTransactions)
            };

            return summary;
        }
        catch (Exception ex)
        {
            // Return an empty summary in case of error
            return new SummaryDto
            {
                CurrentBalance = 0,
                MonthlyIncome = 0,
                MonthlyExpenses = 0,
                MonthlyBalance = 0,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                RecentTransactions = new List<TransactionDto>()
            };
        }
    }
}