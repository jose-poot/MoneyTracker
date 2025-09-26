namespace MoneyTracker.Core.ValueObjects;

/// <summary>
/// Value Object that represents money with business validations.
/// </summary>
/// <remarks>
/// Why a Value Object instead of a plain decimal?
/// - Encapsulates validation logic
/// - Prevents common errors (negative amounts where not allowed)
/// - More expressive than a simple decimal
/// </remarks>
public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    /// <summary>
    /// Constructor that validates the money value.
    /// </summary>
    public Money(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates positive money (for income).
    /// </summary>
    public static Money CreatePositive(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("The amount must be positive", nameof(amount));

        return new Money(amount, currency);
    }

    /// <summary>
    /// Checks if the amount is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Checks if the amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Adds two monetary values.
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts two monetary values.
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Formats the money value for UI display.
    /// </summary>
    public string ToDisplayString()
    {
        return Currency switch
        {
            "USD" => $"${Amount:N2}",
            "EUR" => $"€{Amount:N2}",
            "MXN" => $"${Amount:N2} MXN",
            _ => $"{Amount:N2} {Currency}"
        };
    }

    // IEquatable implementation for comparisons
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => ToDisplayString();

    // Overloaded operators for easier usage
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static bool operator ==(Money left, Money right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Money left, Money right) => !(left == right);
}