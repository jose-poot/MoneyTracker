namespace MoneyTracker.Application.DTOs;

public sealed class UserSettingsDto
{
    public int Id { get; set; }                 // opcional si usas “activo”
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;   // si decides mostrarlo
    public string Currency { get; set; } = "USD";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string Theme { get; set; } = "Light";
    public bool ShowNotifications { get; set; } = true;
}