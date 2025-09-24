using MoneyTracker.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs
{
    /// <summary>
    /// DTO para transferir datos de transacciones a la UI
    /// Contiene solo los datos necesarios para la presentación
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        [Required(ErrorMessage = "El tipo de transacción es obligatorio")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#2196F3";
        public string CategoryIcon { get; set; } = "category";

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? Notes { get; set; }
        public string? Location { get; set; }
        public bool IsRecurring { get; set; }

        // Propiedades calculadas para la UI
        public string FormattedAmount => Type == TransactionType.Income
            ? $"+${Amount:N2}"
            : $"-${Amount:N2}";

        public string FormattedDate => Date.ToString("dd/MM/yyyy");
        public string TypeDisplayName => Type == TransactionType.Income ? "Ingreso" : "Gasto";

        // Para validación en tiempo real en la UI
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}