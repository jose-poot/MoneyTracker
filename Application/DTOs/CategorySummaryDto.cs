namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO representing per-category summaries.
/// </summary>
public class CategorySummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#2196F3";
    public string CategoryIcon { get; set; } = "category";
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; } // Porcentaje del total

    public string FormattedAmount => $"${TotalAmount:N2}";
    public string FormattedPercentage => $"{Percentage:F1}%";
}