using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Presentation.Extensions;
using MoneyTracker.Presentation.Services.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MoneyTracker.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel base optimizado para memoria y performance con servicios inyectados
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject, IDisposable
    {
        // Services (inyectados via constructor en clases derivadas)
        protected readonly IDialogService? DialogService;
        protected readonly INavigationService? NavigationService;
        protected readonly ICacheService? CacheService;

        private readonly CompositeDisposable _subscriptions = new();
        private volatile bool _isDisposed;

        // Cache para PropertyChangedEventArgs para reducir allocations
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _propertyArgsCache = new();

        #region Observable Properties

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

        #endregion

        #region Collections and Validation

        public ObservableCollection<string> ValidationErrors { get; } = new();

        public bool HasValidationErrors => ValidationErrors.Any();

        #endregion

        #region Constructors

        protected BaseViewModel()
        {
            RegisterMessageHandlers();
        }

        protected BaseViewModel(IDialogService dialogService, INavigationService navigationService, ICacheService? cacheService = null)
        {
            DialogService = dialogService;
            NavigationService = navigationService;
            CacheService = cacheService;
            RegisterMessageHandlers();
        }

        #endregion

        #region Virtual Methods

        protected virtual void RegisterMessageHandlers()
        {
            // Override en ViewModels específicos para registrar mensaje handlers
        }

        protected virtual Task OnRefreshRequested() => Task.CompletedTask;

        protected virtual void OnErrorOccurred(string errorMessage)
        {
            // Override para manejo específico de errores
        }

        #endregion

        #region Subscription Management

        protected void AddSubscription(IDisposable subscription)
        {
            if (_isDisposed)
            {
                subscription?.Dispose();
                return;
            }
            _subscriptions.Add(subscription);
        }

        #endregion

        #region Safe Execution Methods

        protected async Task ExecuteSafeAsync(Func<Task> operation, [CallerMemberName] string? operationName = null)
        {
            if (_isDisposed || IsBusy) return;

            operationName ??= "Unknown operation";

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;
                ClearValidationErrors();

                await operation();

                System.Diagnostics.Debug.WriteLine($"✅ Completed {operationName}");
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, operationName);
            }
            finally
            {
                if (!_isDisposed)
                    IsBusy = false;
            }
        }

        protected async Task<T?> ExecuteSafeAsync<T>(Func<Task<T>> operation, [CallerMemberName] string? operationName = null) where T : class
        {
            if (_isDisposed || IsBusy) return null;

            operationName ??= "Unknown operation";

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;
                ClearValidationErrors();

                var result = await operation();

                System.Diagnostics.Debug.WriteLine($"✅ Completed {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, operationName);
                return null;
            }
            finally
            {
                if (!_isDisposed)
                    IsBusy = false;
            }
        }

        #endregion

        #region Error Handling

        private async Task HandleErrorAsync(Exception ex, string operationName)
        {
            if (_isDisposed) return;

            HasError = true;
            ErrorMessage = GetUserFriendlyError(ex);

            System.Diagnostics.Debug.WriteLine($"❌ Error in {GetType().Name}.{operationName}: {ex}");

            // Notify UI services
            if (DialogService != null)
            {
                try
                {
                    await DialogService.ShowErrorAsync(ErrorMessage);
                }
                catch (Exception dialogEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing dialog: {dialogEx.Message}");
                }
            }

            OnErrorOccurred(ErrorMessage);
        }

        private static string GetUserFriendlyError(Exception ex) => ex switch
        {
            ArgumentException => "Los datos ingresados no son válidos",
            InvalidOperationException => "No se puede realizar esta operación en este momento",
            UnauthorizedAccessException => "Sin permisos para realizar esta acción",
            System.Net.Http.HttpRequestException => "Error de conexión. Verifica tu internet",
            TaskCanceledException => "La operación tardó demasiado tiempo",
            TimeoutException => "La operación tardó demasiado tiempo",
            _ => "Ocurrió un error inesperado. Intenta nuevamente"
        };

        #endregion

        #region Validation Methods

        protected void AddValidationError(string error)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(error) || ValidationErrors.Contains(error))
                return;

            ValidationErrors.Add(error);
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        protected void ClearValidationErrors()
        {
            if (_isDisposed || !ValidationErrors.Any())
                return;

            ValidationErrors.Clear();
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        protected void ShowValidationErrors(IEnumerable<string> errors)
        {
            ClearValidationErrors();

            foreach (var error in errors.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                AddValidationError(error);
            }
        }

        #endregion

        #region UI Helper Methods

        protected async Task ShowMessageAsync(string message)
        {
            if (DialogService != null && !string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    DialogService.ShowToast(message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing message: {ex.Message}");
                }
            }
        }

        protected async Task<bool> ShowConfirmAsync(string title, string message)
        {
            if (DialogService != null)
            {
                try
                {
                    return await DialogService.ShowConfirmAsync(title, message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing confirm dialog: {ex.Message}");
                }
            }
            return false;
        }

        protected async Task NavigateBackAsync()
        {
            if (NavigationService != null)
            {
                try
                {
                    await NavigationService.NavigateBackAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error navigating back: {ex.Message}");
                }
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        protected virtual async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await OnRefreshRequested();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, nameof(RefreshAsync));
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        #endregion

        #region Optimized PropertyChanged

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (_isDisposed || propertyName == null) return;

            var args = _propertyArgsCache.GetOrAdd(propertyName, name => new PropertyChangedEventArgs(name));

            try
            {
                base.OnPropertyChanged(propertyName); // Llamar al método base
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PropertyChanged for {propertyName}: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                try
                {
                    _subscriptions.Dispose();
                    ValidationErrors.Clear();

                    // Clear event handlers
                 
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing {GetType().Name}: {ex.Message}");
                }
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Finalizer as safety net
        ~BaseViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}