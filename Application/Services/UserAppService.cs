using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;

namespace MoneyTracker.Application.Services;

public sealed class UserAppService
{
    private readonly IUserRepository _users;

    public UserAppService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserSettingsDto?> GetSettingsAsync(int? userId = null)
    {
        User? user = userId is null
            ? await _users.GetActiveAsync()
            : await _users.GetByIdAsync(userId.Value);

        if (user is null) return null;

        return new UserSettingsDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Currency = user.Currency,
            DateFormat = user.DateFormat ?? "dd/MM/yyyy",
            Theme = user.Theme ?? "Light",
            ShowNotifications = user.ShowNotifications
        };
    }

    public async Task<(bool success, string? error)> UpdateSettingsAsync(UserSettingsDto dto)
    {
        var user = dto.Id > 0
            ? await _users.GetByIdAsync(dto.Id)
            : await _users.GetActiveAsync();

        if (user is null) return (false, "Usuario no encontrado.");

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Length != 3)
            return (false, "La moneda debe tener 3 caracteres (ej: USD).");

        // Mapear cambios
        user.Name = dto.Name.Trim();
        user.Currency = dto.Currency.Trim().ToUpperInvariant();
        user.DateFormat = string.IsNullOrWhiteSpace(dto.DateFormat) ? "dd/MM/yyyy" : dto.DateFormat.Trim();
        user.Theme = string.IsNullOrWhiteSpace(dto.Theme) ? "Light" : dto.Theme.Trim();
        user.ShowNotifications = dto.ShowNotifications;

        await _users.UpdateAsync(user);
        await _users.SaveChangesAsync();

        return (true, null);
    }
}