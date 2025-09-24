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
/// Servicio de aplicación para transacciones
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
    /// Obtiene todas las transacciones
    /// </summary>
    public async Task<List<TransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return _mapper.Map<List<TransactionDto>>(transactions);
    }

    /// <summary>
    /// Obtiene transacciones por rango de fechas
    /// </summary>
    public async Task<List<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<List<TransactionDto>>(transactions);
    }

    /// <summary>
    /// Obtiene una transacción específica
    /// </summary>
    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
    }

    /// <summary>
    /// Crea una nueva transacción con validaciones completas
    /// </summary>
    public async Task<(bool Success, TransactionDto? Transaction, List<string> Errors)> CreateTransactionAsync(CreateTransactionDto createDto)
    {
        // 1. Validar datos de entrada
        var validationResult = await _createValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Convertir DTO a entidad
            var money = new Money(createDto.Amount, createDto.Currency);

            // 3. Usar factory method del dominio para crear transacción correcta
            var transaction = createDto.Type == TransactionType.Expense
                ? Transaction.CreateExpense(createDto.Description, money, createDto.CategoryId, createDto.Date)
                : Transaction.CreateIncome(createDto.Description, money, createDto.CategoryId, createDto.Date);

            // 4. Establecer propiedades adicionales
            transaction.Notes = createDto.Notes;
            transaction.Location = createDto.Location;
            transaction.IsRecurring = createDto.IsRecurring;

            // 5. Validar reglas de dominio
            if (!transaction.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 6. Guardar en repositorio
            var savedTransaction = await _transactionRepository.AddAsync(transaction);

            // 7. Convertir a DTO para retorno
            var resultDto = _mapper.Map<TransactionDto>(savedTransaction);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            // 8. Manejo de errores
            var errorMessage = "Error al crear la transacción: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Actualiza una transacción existente
    /// </summary>
    public async Task<(bool Success, TransactionDto? Transaction, List<string> Errors)> UpdateTransactionAsync(TransactionDto transactionDto)
    {
        // 1. Validar datos
        var validationResult = await _updateValidator.ValidateAsync(transactionDto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return (false, null, errors);
        }

        try
        {
            // 2. Obtener transacción existente
            var existingTransaction = await _transactionRepository.GetByIdAsync(transactionDto.Id);
            if (existingTransaction == null)
            {
                return (false, null, new List<string> { "La transacción no existe" });
            }

            // 3. Actualizar propiedades
            existingTransaction.Description = transactionDto.Description;
            existingTransaction.Amount = new Money(transactionDto.Amount, transactionDto.Currency);
            existingTransaction.CategoryId = transactionDto.CategoryId;
            existingTransaction.Date = transactionDto.Date;
            existingTransaction.Notes = transactionDto.Notes;
            existingTransaction.Location = transactionDto.Location;
            existingTransaction.IsRecurring = transactionDto.IsRecurring;

            // 4. Cambiar tipo si es necesario
            if (existingTransaction.Type != transactionDto.Type)
            {
                existingTransaction.ChangeType(transactionDto.Type);
            }

            existingTransaction.MarkAsUpdated();

            // 5. Validar reglas de dominio
            if (!existingTransaction.IsValid(out var domainErrors))
            {
                return (false, null, domainErrors);
            }

            // 6. Actualizar en repositorio
            var updatedTransaction = await _transactionRepository.UpdateAsync(existingTransaction);

            // 7. Convertir a DTO
            var resultDto = _mapper.Map<TransactionDto>(updatedTransaction);

            return (true, resultDto, new List<string>());
        }
        catch (Exception ex)
        {
            var errorMessage = "Error al actualizar la transacción: " + ex.Message;
            return (false, null, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Elimina una transacción
    /// </summary>
    public async Task<(bool Success, List<string> Errors)> DeleteTransactionAsync(int id)
    {
        try
        {
            // 1. Verificar que existe
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return (false, new List<string> { "La transacción no existe" });
            }

            // 2. Verificar reglas de negocio (puedes agregar más validaciones)
            var canDelete = await _transactionDomainService.CanDeleteTransactionAsync(id);
            if (!canDelete)
            {
                return (false, new List<string> { "No se puede eliminar esta transacción" });
            }

            // 3. Eliminar
            var deleted = await _transactionRepository.DeleteAsync(id);

            return deleted
                ? (true, new List<string>())
                : (false, new List<string> { "Error al eliminar la transacción" });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error al eliminar la transacción: " + ex.Message;
            return (false, new List<string> { errorMessage });
        }
    }

    /// <summary>
    /// Obtiene resumen financiero del mes actual
    /// </summary>
    public async Task<SummaryDto> GetMonthlySummaryAsync(int? year = null, int? month = null)
    {
        var targetDate = new DateTime(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, 1);
        var startDate = targetDate;
        var endDate = startDate.AddMonths(1).AddDays(-1);

        try
        {
            // 1. Obtener transacciones del período
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

            // 2. Calcular totales usando servicio de dominio
            var balance = await _transactionDomainService.GetCurrentBalanceAsync();
            var monthlyIncome = await _transactionDomainService.GetMonthlyIncomeAsync(targetDate.Year, targetDate.Month);
            var monthlyExpenses = await _transactionDomainService.GetMonthlyExpensesAsync(targetDate.Year, targetDate.Month);

            // 3. Obtener categorías más usadas
            var topCategories = await _transactionRepository.GetTopCategoriesAsync(5);

            // 4. Obtener transacciones recientes
            var recentTransactions = await _transactionRepository.GetRecentTransactionsAsync(5);

            // 5. Crear DTO de resumen
            var summary = new SummaryDto
            {
                CurrentBalance = balance.Amount,
                MonthlyIncome = monthlyIncome.Amount,
                MonthlyExpenses = Math.Abs(monthlyExpenses.Amount), // Mostrar como positivo
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
            // En caso de error, retornar un resumen vacío
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