using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services;
using MoneyTracker.Presentation.Services.Interfaces;
using System.Threading.Tasks;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel de Settings (persistencia en DB vía UserAppService)
/// Sigue el mismo patrón que TransactionListViewModel:
/// - [ObservableProperty] en campos privados con _
/// - [RelayCommand] para comandos generados
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly UserAppService _users;

    // ===== Observables (mismo estilo: campos privados con _) =====
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _preferredCurrency = "USD";

    [ObservableProperty]
    private string _preferredDateFormat = "dd/MM/yyyy";

    [ObservableProperty]
    private string _appTheme = "Light";

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    private int _userId;

    public SettingsViewModel(
        UserAppService users,
        IDialogService dialogService,
        INavigationService navigationService)
        : base(dialogService, navigationService)
    {
        _users = users;
        Title = "Settings";
        // Carga inicial al estilo de tu TransactionListViewModel
        _ = LoadAsync();
    }

    #region Comandos

    /// <summary>
    /// Cargar datos de usuario desde DB
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var dto = await _users.GetSettingsAsync();
            if (dto is null) return;

            _userId = dto.Id;
            UserName = dto.Name ?? string.Empty;
            PreferredCurrency = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency;
            PreferredDateFormat = string.IsNullOrWhiteSpace(dto.DateFormat) ? "dd/MM/yyyy" : dto.DateFormat;
            AppTheme = string.IsNullOrWhiteSpace(dto.Theme) ? "Light" : dto.Theme;
            NotificationsEnabled = dto.ShowNotifications;
        });
    }

    /// <summary>
    /// Guardar cambios en DB
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var name = (UserName ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                if (DialogService != null)
                {
                    await DialogService.ShowErrorAsync("Por favor ingresa un nombre de usuario.");
                }
                return;
            }

            var dto = new UserSettingsDto
            {
                Id = _userId,
                Name = name,
                Currency = (PreferredCurrency ?? "USD").Trim().ToUpperInvariant(),
                DateFormat = string.IsNullOrWhiteSpace(PreferredDateFormat) ? "dd/MM/yyyy" : PreferredDateFormat.Trim(),
                Theme = string.IsNullOrWhiteSpace(AppTheme) ? "Light" : AppTheme.Trim(),
                ShowNotifications = NotificationsEnabled
            };

            var (success, error) = await _users.UpdateSettingsAsync(dto);
            if (!success)
            {
                if (DialogService != null)
                {
                    await DialogService.ShowErrorAsync(error ?? "No se pudo guardar.");
                }
                return;
            }

            DialogService?.ShowToast("Settings guardados.");
        });
    }

    #endregion
}