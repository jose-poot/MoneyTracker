using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;

namespace MoneyTracker.Application.Extensions;

/// <summary>
/// Extension methods for DTOs.
/// </summary>
public static class DtoExtensions
{
    /// <summary>
    /// Filters transactions by type.
    /// </summary>
    public static List<TransactionDto> OfType(this IEnumerable<TransactionDto> transactions, TransactionType type)
    {
        return transactions.Where(t => t.Type == type).ToList();
    }

    /// <summary>
    /// Filters transactions by category.
    /// </summary>
    public static List<TransactionDto> OfCategory(this IEnumerable<TransactionDto> transactions, int categoryId)
    {
        return transactions.Where(t => t.CategoryId == categoryId).ToList();
    }

    /// <summary>
    /// Filters transactions by date range.
    /// </summary>
    public static List<TransactionDto> InDateRange(this IEnumerable<TransactionDto> transactions, DateTime startDate, DateTime endDate)
    {
        return transactions.Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date).ToList();
    }

    /// <summary>
    /// Orders transactions by descending date.
    /// </summary>
    public static List<TransactionDto> OrderByDateDescending(this IEnumerable<TransactionDto> transactions)
    {
        return transactions.OrderByDescending(t => t.Date).ToList();
    }

    /// <summary>
    /// Calculates the total amount for a group of transactions.
    /// </summary>
    public static decimal GetTotal(this IEnumerable<TransactionDto> transactions)
    {
        return transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
    }

    /// <summary>
    /// Groups transactions by month.
    /// </summary>
    public static Dictionary<string, List<TransactionDto>> GroupByMonth(this IEnumerable<TransactionDto> transactions)
    {
        return transactions
            .GroupBy(t => t.Date.ToString("yyyy-MM"))
            .ToDictionary(
                g => DateTime.ParseExact(g.Key, "yyyy-MM", null).ToString("MMMM yyyy"),
                g => g.ToList()
            );
    }

    /// <summary>
    /// Converts to a format used for charts.
    /// </summary>
    public static List<ChartDataDto> ToChartData(this IEnumerable<CategorySummaryDto> categories)
    {
        return categories.Select(c => new ChartDataDto
        {
            Label = c.CategoryName,
            Value = (float)c.TotalAmount,
            Color = c.CategoryColor
        }).ToList();
    }

}