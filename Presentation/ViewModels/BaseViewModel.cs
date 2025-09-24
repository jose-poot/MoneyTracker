using Android.Telephony;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel base con funcionalidad común para todas las pantallas
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// Ejecuta una operación de forma segura con manejo de errores
    /// </summary>
    protected async Task ExecuteSafeAsync(Func<Task> operation, string? loadingMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = GetUserFriendlyError(ex);
            System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {ex}");

            // Notificar error para mostrar en UI
            OnErrorOccurred(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Versión genérica para operaciones que retornan datos
    /// </summary>
    protected async Task<T?> ExecuteSafeAsync<T>(Func<Task<T>> operation) where T : class
    {
        if (IsBusy) return null;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            return await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = GetUserFriendlyError(ex);
            System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {ex}");
            OnErrorOccurred(ErrorMessage);
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Convierte excepciones técnicas en mensajes amigables para el usuario
    /// </summary>
    private static string GetUserFriendlyError(Exception ex)
    {
        return ex switch
        {
            ArgumentException => "Los datos ingresados no son válidos",
            InvalidOperationException => "No se puede realizar esta operación en este momento",
            UnauthorizedAccessException => "No tienes permisos para realizar esta acción",
            System.Net.Http.HttpRequestException => "Error de conexión. Verifica tu internet",
            TaskCanceledException => "La operación tardó demasiado tiempo",
            _ => "Ocurrió un error inesperado. Intenta nuevamente"
        };
    }

    /// <summary>
    /// Evento para notificar errores a la UI
    /// </summary>
    public event Action<string>? ErrorOccurred;

    protected virtual void OnErrorOccurred(string errorMessage)
    {
        ErrorOccurred?.Invoke(errorMessage);
    }

    /// <summary>
    /// Comando para limpiar errores
    /// </summary>
    [RelayCommand]
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Comando genérico para refresh
    /// </summary>
    [RelayCommand]
    public virtual async Task RefreshAsync()
    {
        // Override en ViewModels específicos
        await Task.CompletedTask;
    }
}