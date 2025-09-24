namespace MoneyTracker.Core.Entities;

/// <summary>
/// Usuario del sistema (para futuras funcionalidades)
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Configuraciones del usuario
    public bool ShowNotifications { get; set; } = true;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string Theme { get; set; } = "Light"; // Light, Dark

    // Relaciones
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    /// <summary>
    /// Valida los datos del usuario
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("El nombre es obligatorio");

        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("El email es obligatorio");
        else if (!IsValidEmail(Email))
            errors.Add("El formato del email no es válido");

        return errors.Count == 0;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Name;
}