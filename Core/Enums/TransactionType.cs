namespace MoneyTracker.Core.Enums;

/// <summary>
/// Tipos de transacciones que maneja la aplicación
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Dinero que entra (salario, ventas, regalos)
    /// </summary>
    Income = 1,

    /// <summary>
    /// Dinero que sale (compras, servicios, entretenimiento)
    /// </summary>
    Expense = 2
}