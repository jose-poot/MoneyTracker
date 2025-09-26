namespace MoneyTracker.Core.Enums;

/// <summary>
/// Transaction types handled by the application.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Money coming in (salary, sales, gifts).
    /// </summary>
    Income = 1,

    /// <summary>
    /// Money going out (shopping, services, entertainment).
    /// </summary>
    Expense = 2
}