using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Entities;

/// <summary>
/// Transacción financiera (ingreso o gasto)
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Value Object para manejar dinero de forma segura
    private Money _amount = new(0);
    public Money Amount
    {
        get => _amount;
        set => _amount = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Para compatibilidad con Entity Framework
    public decimal AmountValue
    {
        get => Amount.Amount;
        set => _amount = new Money(value, Amount?.Currency ?? "USD");
    }

    public string Currency
    {
        get => Amount.Currency;
        set => _amount = new Money(Amount.Amount, value);
    }

    // Relación con categoría
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    // Campos opcionales para funcionalidad avanzada
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Valida que la transacción tenga datos válidos según reglas de negocio
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        // Regla 1: Descripción obligatoria
        if (string.IsNullOrWhiteSpace(Description))
            errors.Add("La descripción es obligatoria");

        // Regla 2: Descripción no muy larga
        if (Description.Length > 200)
            errors.Add("La descripción no puede tener más de 200 caracteres");

        // Regla 3: Monto válido según tipo de transacción
        if (Type == TransactionType.Expense && Amount.Amount > 0)
            errors.Add("Los gastos deben tener monto negativo o usar CreateExpense()");

        if (Type == TransactionType.Income && Amount.Amount < 0)
            errors.Add("Los ingresos deben tener monto positivo");

        // Regla 4: Monto no puede ser cero
        if (Amount.IsZero)
            errors.Add("El monto no puede ser cero");

        // Regla 5: Fecha no puede ser futura para gastos reales
        if (Date > DateTime.UtcNow.AddDays(1) && !IsRecurring)
            errors.Add("La fecha no puede ser futura para transacciones reales");

        // Regla 6: Debe tener categoría
        if (CategoryId <= 0)
            errors.Add("Debe seleccionar una categoría");

        return errors.Count == 0;
    }

    /// <summary>
    /// Factory method para crear un gasto
    /// </summary>
    public static Transaction CreateExpense(string description, Money amount, int categoryId, DateTime? date = null)
    {
        // Convertimos a monto negativo para gastos
        var expenseAmount = new Money(-Math.Abs(amount.Amount), amount.Currency);

        return new Transaction
        {
            Description = description,
            Amount = expenseAmount,
            Type = TransactionType.Expense,
            CategoryId = categoryId,
            Date = date ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method para crear un ingreso
    /// </summary>
    public static Transaction CreateIncome(string description, Money amount, int categoryId, DateTime? date = null)
    {
        // Los ingresos siempre son positivos
        var incomeAmount = new Money(Math.Abs(amount.Amount), amount.Currency);

        return new Transaction
        {
            Description = description,
            Amount = incomeAmount,
            Type = TransactionType.Income,
            CategoryId = categoryId,
            Date = date ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Indica si es un gasto
    /// </summary>
    public bool IsExpense => Type == TransactionType.Expense;

    /// <summary>
    /// Indica si es un ingreso
    /// </summary>
    public bool IsIncome => Type == TransactionType.Income;

    /// <summary>
    /// Obtiene el monto absoluto (sin signo)
    /// </summary>
    public Money GetAbsoluteAmount() => new(Math.Abs(Amount.Amount), Amount.Currency);

    /// <summary>
    /// Cambia el tipo de transacción ajustando el signo del monto
    /// </summary>
    public void ChangeType(TransactionType newType)
    {
        if (Type == newType) return;

        Type = newType;
        var absoluteAmount = Math.Abs(Amount.Amount);

        Amount = newType == TransactionType.Expense
            ? new Money(-absoluteAmount, Amount.Currency)
            : new Money(absoluteAmount, Amount.Currency);

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca la transacción como modificada
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"{Type}: {Description} - {Amount}";
}