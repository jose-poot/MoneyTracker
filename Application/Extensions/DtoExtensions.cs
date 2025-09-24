using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;

namespace MoneyTracker.Application.Extensions;

/// <summary>
/// Métodos de extensión para DTOs
/// </summary>
public static class DtoExtensions
{
    /// <summary>
    /// Filtra transacciones por tipo
    /// </summary>
    public static List<TransactionDto> OfType(this IEnumerable<TransactionDto> transactions, TransactionType type)
    {
        return transactions.Where(t => t.Type == type).ToList();
    }

    /// <summary>
    /// Filtra transacciones por categoría
    /// </summary>
    public static List<TransactionDto> OfCategory(this IEnumerable<TransactionDto> transactions, int categoryId)
    {
        return transactions.Where(t => t.CategoryId == categoryId).ToList();
    }

    /// <summary>
    /// Filtra transacciones por rango de fechas
    /// </summary>
    public static List<TransactionDto> InDateRange(this IEnumerable<TransactionDto> transactions, DateTime startDate, DateTime endDate)
    {
        return transactions.Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date).ToList();
    }

    /// <summary>
    /// Ordena transacciones por fecha descendente
    /// </summary>
    public static List<TransactionDto> OrderByDateDescending(this IEnumerable<TransactionDto> transactions)
    {
        return transactions.OrderByDescending(t => t.Date).ToList();
    }

    /// <summary>
    /// Calcula el total de un grupo de transacciones
    /// </summary>
    public static decimal GetTotal(this IEnumerable<TransactionDto> transactions)
    {
        return transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
    }

    /// <summary>
    /// Agrupa transacciones por mes
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
    /// Convierte a formato para gráficos
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