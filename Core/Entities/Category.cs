namespace MoneyTracker.Core.Entities;

/// <summary>
/// Categoría para clasificar transacciones (Comida, Transporte, etc.)
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196F3"; // Color en formato hexadecimal
    public string Icon { get; set; } = "category"; // Nombre del icono
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relación: Una categoría puede tener muchas transacciones
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Valida que la categoría tenga datos válidos
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("El nombre de la categoría es obligatorio");

        if (Name.Length > 50)
            errors.Add("El nombre no puede tener más de 50 caracteres");

        if (!IsValidColor(Color))
            errors.Add("El color debe estar en formato hexadecimal (#RRGGBB)");

        return errors.Count == 0;
    }

    private static bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;

        // Verifica que sean caracteres hexadecimales válidos
        for (int i = 1; i < color.Length; i++)
        {
            char c = color[i];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Crea categorías predeterminadas del sistema
    /// </summary>
    public static List<Category> GetDefaultCategories()
    {
        return new List<Category>
            {
                new() { Name = "Alimentación", Description = "Comida y bebidas", Color = "#FF9800", Icon = "restaurant" },
                new() { Name = "Transporte", Description = "Gasolina, transporte público", Color = "#2196F3", Icon = "directions_car" },
                new() { Name = "Entretenimiento", Description = "Cine, juegos, salidas", Color = "#E91E63", Icon = "movie" },
                new() { Name = "Servicios", Description = "Luz, agua, internet", Color = "#4CAF50", Icon = "build" },
                new() { Name = "Salud", Description = "Medicinas, consultas", Color = "#F44336", Icon = "local_hospital" },
                new() { Name = "Educación", Description = "Libros, cursos", Color = "#9C27B0", Icon = "school" },
                new() { Name = "Ingresos", Description = "Salario, ventas", Color = "#4CAF50", Icon = "attach_money" }
            };
    }

    public override string ToString() => Name;
}