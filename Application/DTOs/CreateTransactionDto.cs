using MoneyTracker.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO used to create new transactions.
/// Contains only the fields required for creation.
/// </summary>
public class CreateTransactionDto
{
    [Required(ErrorMessage = "The description is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "The description must be between 3 and 200 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "The amount is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "The amount must be between $0.01 and $999,999,999.99")]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    [Required(ErrorMessage = "The transaction type must be selected")]
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "A category must be selected")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid category must be selected")]
    public int CategoryId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    [StringLength(100, ErrorMessage = "The location cannot exceed 100 characters")]
    public string? Location { get; set; }

    public bool IsRecurring { get; set; } = false;
}