namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO para mostrar resumen financiero en dashboard
/// </summary>
public class SummaryDto
{
    public decimal CurrentBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyBalance { get; set; }
    public string Currency { get; set; } = "USD";

    // Formateo para UI
    public string FormattedBalance => FormatMoney(CurrentBalance);
    public string FormattedIncome => FormatMoney(MonthlyIncome);
    public string FormattedExpenses => FormatMoney(MonthlyExpenses);
    public string FormattedMonthlyBalance => FormatMoney(MonthlyBalance);

    // Estadísticas adicionales
    public int TotalTransactionsThisMonth { get; set; }
    public int TotalCategoriesUsed { get; set; }

    // Top categorías del mes
    public List<CategorySummaryDto> TopExpenseCategories { get; set; } = new();
    public List<CategorySummaryDto> TopIncomeCategories { get; set; } = new();

    // Transacciones recientes
    public List<TransactionDto> RecentTransactions { get; set; } = new();

    // Período del resumen
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodDescription => $"{PeriodStart:MMMM yyyy}";

    // Indicadores visuales para la UI
    public bool HasPositiveBalance => CurrentBalance > 0;
    public bool HasMonthlyProfit => MonthlyBalance > 0;
    public string BalanceColor => HasPositiveBalance ? "#4CAF50" : "#F44336";
    public string MonthlyBalanceColor => HasMonthlyProfit ? "#4CAF50" : "#F44336";

    private string FormatMoney(decimal amount)
    {
        return amount >= 0 ? $"${amount:N2}" : $"-${Math.Abs(amount):N2}";
    }
}