namespace MoneyTracker.Core.ValueObjects;

/// <summary>
/// Value Object que representa dinero con validaciones de negocio
/// </summary>
/// <remarks>
/// ¿Por qué un Value Object y no solo decimal?
/// - Encapsula lógica de validación
/// - Evita errores comunes (dinero negativo donde no debe)
/// - Más expresivo que un decimal simple
/// </remarks>
public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    /// <summary>
    /// Constructor que valida que el dinero sea válido
    /// </summary>
    public Money(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("La moneda no puede estar vacía", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Crea dinero positivo (para ingresos)
    /// </summary>
    public static Money CreatePositive(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("El monto debe ser positivo", nameof(amount));

        return new Money(amount, currency);
    }

    /// <summary>
    /// Verifica si el monto es positivo
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Verifica si el monto es cero
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Suma dos cantidades de dinero
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"No se pueden sumar monedas diferentes: {Currency} y {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Resta dos cantidades de dinero  
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"No se pueden restar monedas diferentes: {Currency} y {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Formatea el dinero para mostrar en UI
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

    // Implementación de IEquatable para comparaciones
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => ToDisplayString();

    // Operadores sobrecargados para facilitar el uso
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static bool operator ==(Money left, Money right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Money left, Money right) => !(left == right);
}