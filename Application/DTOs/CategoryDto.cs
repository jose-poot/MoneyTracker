using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO for categories - UI-specific data.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The category name is required")]
    [StringLength(50, ErrorMessage = "The name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "The description cannot exceed 200 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "The color must be in hexadecimal format (#RRGGBB)")]
    public string Color { get; set; } = "#2196F3";

    public string Icon { get; set; } = "category";
    public bool IsActive { get; set; } = true;

    // Calculated properties for statistics
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string FormattedTotal => $"${TotalAmount:N2}";

    // UI helpers
    public bool IsSelected { get; set; } // For multi-selection
    public bool CanDelete { get; set; } // Cannot be removed if it has transactions

    // Override ToString() so the Spinner displays the name
    public override string ToString()
    {
        return Name;
    }
}