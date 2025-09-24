using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs;

/// <summary>
/// DTO para categorías - datos específicos para la UI
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede tener más de 200 caracteres")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "El color debe ser en formato hexadecimal (#RRGGBB)")]
    public string Color { get; set; } = "#2196F3";

    public string Icon { get; set; } = "category";
    public bool IsActive { get; set; } = true;

    // Propiedades calculadas para estadísticas
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string FormattedTotal => $"${TotalAmount:N2}";

    // Para UI
    public bool IsSelected { get; set; } // Para selección múltiple
    public bool CanDelete { get; set; } // Si tiene transacciones, no se puede borrar

    // Sobrescribir ToString() para que el Spinner muestre el nombre
    public override string ToString()
    {
        return Name;
    }
}