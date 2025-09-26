namespace MoneyTracker.Core.Entities;

/// <summary>
/// Category used to classify transactions (Food, Transport, etc.).
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196F3"; // Hexadecimal color value
    public string Icon { get; set; } = "category"; // Icon name
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationship: A category can have many transactions
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Validates that the category contains valid data.
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("The category name is required");

        if (Name.Length > 50)
            errors.Add("The name cannot exceed 50 characters");

        if (!IsValidColor(Color))
            errors.Add("Color must be in hexadecimal format (#RRGGBB)");

        return errors.Count == 0;
    }

    private static bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;

        // Ensure the characters are valid hexadecimal digits
        for (int i = 1; i < color.Length; i++)
        {
            char c = color[i];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates the system default categories.
    /// </summary>
    public static List<Category> GetDefaultCategories()
    {
        return new List<Category>
        {
            new() { Name = "Food", Description = "Meals and beverages", Color = "#FF9800", Icon = "restaurant" },
            new() { Name = "Transport", Description = "Fuel, public transport", Color = "#2196F3", Icon = "directions_car" },
            new() { Name = "Entertainment", Description = "Movies, games, outings", Color = "#E91E63", Icon = "movie" },
            new() { Name = "Utilities", Description = "Electricity, water, internet", Color = "#4CAF50", Icon = "build" },
            new() { Name = "Health", Description = "Medicine, appointments", Color = "#F44336", Icon = "local_hospital" },
            new() { Name = "Education", Description = "Books, courses", Color = "#9C27B0", Icon = "school" },
            new() { Name = "Income", Description = "Salary, sales", Color = "#4CAF50", Icon = "attach_money" }
        };
    }

    public override string ToString() => Name;
}