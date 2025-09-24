using MoneyTracker.Core.Entities;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Services;

/// <summary>
/// Servicio de dominio para cálculos financieros complejos
/// </summary>
public class MoneyCalculatorService
{
    /// <summary>
    /// Calcula el balance total de una lista de transacciones
    /// </summary>
    public Money CalculateBalance(IEnumerable<Transaction> transactions, string currency = "USD")
    {
        if (!transactions.Any())
            return new Money(0, currency);

        var total = decimal.Zero;

        foreach (var transaction in transactions)
        {
            // Los ingresos suman, los gastos restan
            if (transaction.Type == Core.Enums.TransactionType.Income)
                total += Math.Abs(transaction.Amount.Amount);
            else
                total -= Math.Abs(transaction.Amount.Amount);
        }

        return new Money(total, currency);
    }

    /// <summary>
    /// Calcula estadísticas de un período
    /// </summary>
    public (Money TotalIncome, Money TotalExpenses, Money Balance) CalculatePeriodStats(
        IEnumerable<Transaction> transactions,
        string currency = "USD")
    {
        var transactionList = transactions.ToList();

        var totalIncome = new Money(
            transactionList
                .Where(t => t.Type == Core.Enums.TransactionType.Income)
                .Sum(t => Math.Abs(t.Amount.Amount)),
            currency
        );

        var totalExpenses = new Money(
            transactionList
                .Where(t => t.Type == Core.Enums.TransactionType.Expense)
                .Sum(t => Math.Abs(t.Amount.Amount)),
            currency
        );

        var balance = totalIncome - totalExpenses;

        return (totalIncome, totalExpenses, balance);
    }

    /// <summary>
    /// Agrupa transacciones por categoría con totales
    /// </summary>
    public Dictionary<Category, Money> GroupByCategory(
        IEnumerable<Transaction> transactions,
        string currency = "USD")
    {
        return transactions
            .GroupBy(t => t.Category)
            .ToDictionary(
                g => g.Key,
                g => new Money(g.Sum(t => Math.Abs(t.Amount.Amount)), currency)
            );
    }
}