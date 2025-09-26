using MoneyTracker.Core.Enums;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Entities;

/// <summary>
/// Financial transaction (income or expense).
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Value Object used to handle money safely
    private Money _amount = new(0);
    public Money Amount
    {
        get => _amount;
        set => _amount = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Required for Entity Framework compatibility
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

    // Relationship with category
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    // Optional fields for advanced functionality
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public bool IsRecurring { get; set; } = false;

    /// <summary>
    /// Validates that the transaction contains valid data according to business rules.
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        // Rule 1: Description is required
        if (string.IsNullOrWhiteSpace(Description))
            errors.Add("The description is required");

        // Rule 2: Description not too long
        if (Description.Length > 200)
            errors.Add("The description cannot exceed 200 characters");

        // Rule 3: Amount must be valid for the transaction type
        if (Type == TransactionType.Expense && Amount.Amount > 0)
            errors.Add("Expenses must have a negative amount or use CreateExpense().");

        if (Type == TransactionType.Income && Amount.Amount < 0)
            errors.Add("Income must have a positive amount.");

        // Rule 4: Amount cannot be zero
        if (Amount.IsZero)
            errors.Add("The amount cannot be zero");

        // Rule 5: The date cannot be in the future for actual transactions
        if (Date > DateTime.UtcNow.AddDays(1) && !IsRecurring)
            errors.Add("The date cannot be in the future for actual transactions");

        // Rule 6: Must have a category
        if (CategoryId <= 0)
            errors.Add("A category must be selected");

        return errors.Count == 0;
    }

    /// <summary>
    /// Factory method used to create an expense transaction.
    /// </summary>
    public static Transaction CreateExpense(string description, Money amount, int categoryId, DateTime? date = null)
    {
        // Force a negative amount for expenses
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
    /// Factory method used to create an income transaction.
    /// </summary>
    public static Transaction CreateIncome(string description, Money amount, int categoryId, DateTime? date = null)
    {
        // Income amounts are always positive
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
    /// Indicates whether the transaction is an expense.
    /// </summary>
    public bool IsExpense => Type == TransactionType.Expense;

    /// <summary>
    /// Indicates whether the transaction is an income.
    /// </summary>
    public bool IsIncome => Type == TransactionType.Income;

    /// <summary>
    /// Returns the absolute amount (without sign).
    /// </summary>
    public Money GetAbsoluteAmount() => new(Math.Abs(Amount.Amount), Amount.Currency);

    /// <summary>
    /// Changes the transaction type adjusting the amount sign.
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
    /// Marks the transaction as modified.
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"{Type}: {Description} - {Amount}";
}