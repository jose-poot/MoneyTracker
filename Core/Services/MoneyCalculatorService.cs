using MoneyTracker.Core.Entities;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Core.Services;

/// <summary>
/// Domain service for complex financial calculations.
/// </summary>
public class MoneyCalculatorService
{
    /// <summary>
    /// Calculates the total balance for a list of transactions.
    /// </summary>
    public Money CalculateBalance(IEnumerable<Transaction> transactions, string currency = "USD")
    {
        if (!transactions.Any())
            return new Money(0, currency);

        var total = decimal.Zero;

        foreach (var transaction in transactions)
        {
            // Income adds, expenses subtract
            if (transaction.Type == Core.Enums.TransactionType.Income)
                total += Math.Abs(transaction.Amount.Amount);
            else
                total -= Math.Abs(transaction.Amount.Amount);
        }

        return new Money(total, currency);
    }

    /// <summary>
    /// Calculates statistics for a period.
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
    /// Groups transactions by category with totals.
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