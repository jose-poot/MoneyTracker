namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO used to present the financial summary on the dashboard.
/// </summary>
public class SummaryDto
{
    public decimal CurrentBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyBalance { get; set; }
    public string Currency { get; set; } = "USD";

    // UI formatting helpers
    public string FormattedBalance => FormatMoney(CurrentBalance);
    public string FormattedIncome => FormatMoney(MonthlyIncome);
    public string FormattedExpenses => FormatMoney(MonthlyExpenses);
    public string FormattedMonthlyBalance => FormatMoney(MonthlyBalance);

    // Additional statistics
    public int TotalTransactionsThisMonth { get; set; }
    public int TotalCategoriesUsed { get; set; }

    // Top categories of the month
    public List<CategorySummaryDto> TopExpenseCategories { get; set; } = new();
    public List<CategorySummaryDto> TopIncomeCategories { get; set; } = new();

    // Recent transactions
    public List<TransactionDto> RecentTransactions { get; set; } = new();

    // Summary period
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodDescription => $"{PeriodStart:MMMM yyyy}";

    // Visual indicators for the UI
    public bool HasPositiveBalance => CurrentBalance > 0;
    public bool HasMonthlyProfit => MonthlyBalance > 0;
    public string BalanceColor => HasPositiveBalance ? "#4CAF50" : "#F44336";
    public string MonthlyBalanceColor => HasMonthlyProfit ? "#4CAF50" : "#F44336";

    private string FormatMoney(decimal amount)
    {
        return amount >= 0 ? $"${amount:N2}" : $"-${Math.Abs(amount):N2}";
    }
}