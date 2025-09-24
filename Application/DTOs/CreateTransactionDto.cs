using MoneyTracker.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO específico para crear nuevas transacciones
/// Contiene solo los campos necesarios para la creación
/// </summary>
public class CreateTransactionDto
{
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "La descripción debe tener entre 3 y 200 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, 999999999.99, ErrorMessage = "El monto debe estar entre $0.01 y $999,999,999.99")]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    [Required(ErrorMessage = "Debe seleccionar el tipo de transacción")]
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una categoría")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int CategoryId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [StringLength(500, ErrorMessage = "Las notas no pueden tener más de 500 caracteres")]
    public string? Notes { get; set; }

    [StringLength(100, ErrorMessage = "La ubicación no puede tener más de 100 caracteres")]
    public string? Location { get; set; }

    public bool IsRecurring { get; set; } = false;
}