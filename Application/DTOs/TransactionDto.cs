using MoneyTracker.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs
{
    /// <summary>
    /// DTO used to transfer transaction data to the UI.
    /// Contains only the data required for presentation.
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The description is required")]
        [StringLength(200, ErrorMessage = "The description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "The amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be greater than zero")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        [Required(ErrorMessage = "The transaction type is required")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "A category must be selected")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#2196F3";
        public string CategoryIcon { get; set; } = "category";

        [Required(ErrorMessage = "The date is required")]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? Notes { get; set; }
        public string? Location { get; set; }
        public bool IsRecurring { get; set; }

        // Calculated properties for the UI
        public string FormattedAmount => Type == TransactionType.Income
            ? $"+${Amount:N2}"
            : $"-${Amount:N2}";

        public string FormattedDate => Date.ToString("dd/MM/yyyy");
        public string TypeDisplayName => Type == TransactionType.Income ? "Income" : "Expense";

        // Used for real-time UI validation
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}