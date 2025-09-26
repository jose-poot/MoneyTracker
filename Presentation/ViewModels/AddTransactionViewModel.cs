using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel para agregar/editar transacciones
/// </summary>
public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly TransactionAppService _transactionService;
    private readonly CategoryAppService _categoryService;
    public AddTransactionViewModel(
        TransactionAppService transactionService,
        CategoryAppService categoryService,
        IDialogService dialogService,
        INavigationService navigationService)
        : base(dialogService, navigationService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;

        Title = "Nueva Transacción";
        Categories = new ObservableCollection<CategoryDto>();

        TransactionDate = DateTime.Now;
        TransactionType = TransactionType.Expense;
        Currency = "USD";

        _ = LoadCategoriesAsync();
    }

   

    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _currency = "USD";
    [ObservableProperty] private TransactionType _transactionType = TransactionType.Expense;
    [ObservableProperty] private DateTime _transactionDate = DateTime.Now;
    [ObservableProperty] private CategoryDto? _selectedCategory;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private bool _isRecurring;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private int _transactionId;
    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private bool _canSave;

    public ObservableCollection<CategoryDto> Categories { get; }

    #region Propiedades Calculadas

    /// <summary>
    /// Texto del botón principal
    /// </summary>
    public string SaveButtonText => IsEditMode ? "Actualizar" : "Guardar";

    /// <summary>
    /// Monto formateado para mostrar
    /// </summary>
    public string FormattedAmount => Amount > 0 ? $"${Amount:N2}" : "$0.00";

    /// <summary>
    /// Indica si es un gasto (para cambiar colores de UI)
    /// </summary>
    public bool IsExpense => TransactionType == TransactionType.Expense;

    /// <summary>
    /// Color del tipo de transacción
    /// </summary>
    public string TypeColor => IsExpense ? "#F44336" : "#4CAF50";

    #endregion

    #region Comandos

    /// <summary>
    /// Carga las categorías disponibles
    /// </summary>
    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            // Seleccionar primera categoría por defecto si no hay ninguna seleccionada
            if (SelectedCategory == null && Categories.Any())
            {
                SelectedCategory = Categories.First();
            }
        });
    }

    /// <summary>
    /// Cambia el tipo de transacción
    /// </summary>
    [RelayCommand]
    private void ToggleTransactionType()
    {
        TransactionType = TransactionType == TransactionType.Expense
            ? TransactionType.Income
            : TransactionType.Expense;

        OnPropertyChanged(nameof(IsExpense));
        OnPropertyChanged(nameof(TypeColor));

        ValidateForm();
    }

    /// <summary>
    /// Guarda la transacción
    /// </summary>
    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (!ValidateForm()) return;

        await ExecuteSafeAsync(async () =>
        {
            if (IsEditMode)
            {
                await UpdateTransactionAsync();
            }
            else
            {
                await CreateTransactionAsync();
            }
        });
    }

    /// <summary>
    /// Cancela la operación
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        if (NavigationService != null)
        {
            await NavigationService.NavigateBackAsync();
        }
    }


    /// <summary>
    /// Limpia el formulario
    /// </summary>
    [RelayCommand]
    private void ClearForm()
    {
        Description = string.Empty;
        Amount = 0;
        TransactionDate = DateTime.Now;
        SelectedCategory = Categories.FirstOrDefault();
        Notes = string.Empty;
        Location = string.Empty;
        IsRecurring = false;

        ClearValidationErrors();
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Configura el ViewModel para editar una transacción existente
    /// </summary>
    public void SetEditMode(TransactionDto transaction)
    {
        IsEditMode = true;
        TransactionId = transaction.Id;
        Title = "Editar Transacción";

        // Cargar datos de la transacción
        Description = transaction.Description;
        Amount = transaction.Amount;
        Currency = transaction.Currency;
        TransactionType = transaction.Type;
        TransactionDate = transaction.Date;
        Notes = transaction.Notes ?? string.Empty;
        Location = transaction.Location ?? string.Empty;
        IsRecurring = transaction.IsRecurring;

        // Seleccionar categoría correspondiente
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == transaction.CategoryId);

        ValidateForm();
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Crea una nueva transacción
    /// </summary>
    private async Task CreateTransactionAsync()
    {
        var createDto = new CreateTransactionDto
        {
            Description = Description.Trim(),
            Amount = Amount,
            Currency = Currency,
            Type = TransactionType,
            CategoryId = SelectedCategory!.Id,
            Date = TransactionDate,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            IsRecurring = IsRecurring
        };

        var (success, transaction, errors) = await _transactionService.CreateTransactionAsync(createDto);

        if (success && transaction != null)
        {
            DialogService?.ShowToast("Transacción creada correctamente");
            if (NavigationService != null)
            {
                await NavigationService.NavigateBackAsync();
            }
        }
        else
        {
            if (DialogService != null)
            {
                await DialogService.ShowErrorAsync("Error al crear la transacción");
            }
            await ShowValidationErrorsAsync(errors);
        }
    }

    /// <summary>
    /// Actualiza una transacción existente
    /// </summary>
    private async Task UpdateTransactionAsync()
    {
        var updateDto = new TransactionDto
        {
            Id = TransactionId,
            Description = Description.Trim(),
            Amount = Amount,
            Currency = Currency,
            Type = TransactionType,
            CategoryId = SelectedCategory!.Id,
            Date = TransactionDate,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            IsRecurring = IsRecurring
        };

        var (success, transaction, errors) = await _transactionService.UpdateTransactionAsync(updateDto);

        if (success && transaction != null)
        {
            DialogService?.ShowToast("Transacción actualizada correctamente");
            if (NavigationService != null)
            {
                await NavigationService.NavigateBackAsync();
            }
        }
        else
        {
            await ShowValidationErrorsAsync(errors);
        }
    }

    /// <summary>
    /// Valida el formulario completo
    /// </summary>
    private bool ValidateForm()
    {
        ClearValidationErrors();

        // Validar descripción
        if (string.IsNullOrWhiteSpace(Description))
            AddValidationError("La descripción es obligatoria");
        else if (Description.Trim().Length < 3)
            AddValidationError("La descripción debe tener al menos 3 caracteres");

        // Validar monto
        if (Amount <= 0)
            AddValidationError("El monto debe ser mayor a cero");
        else if (Amount > 999999999)
            AddValidationError("El monto es demasiado grande");

        // Validar categoría
        if (SelectedCategory == null)
            AddValidationError("Debe seleccionar una categoría");

        // Validar fecha
        if (TransactionDate > DateTime.Now.AddDays(1))
            AddValidationError("La fecha no puede ser futura");

        // Validar notas (opcional)
        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
            AddValidationError("Las notas no pueden tener más de 500 caracteres");

        CanSave = !HasValidationErrors;
        return CanSave;
    }

    protected new void AddValidationError(string error)
    {
        if (!ValidationErrors.Contains(error))
        {
            ValidationErrors.Add(error);
            HasValidationErrors = true;
        }
    }
    private new void ClearValidationErrors()
    {
        ValidationErrors.Clear();
        HasValidationErrors = false;
    }

    private async Task ShowValidationErrorsAsync(System.Collections.Generic.List<string>? errors)
    {
        ClearValidationErrors();

        if (errors == null || errors.Count == 0)
        {
            AddValidationError("Ocurrió un error inesperado");
            errors = ValidationErrors.ToList();
        }

        foreach (var error in errors)
            AddValidationError(error);

        if (DialogService != null)
        {
            await DialogService.ShowErrorAsync("Por favor corrige los errores indicados");
        }
    }

    #endregion

    #region Event Handlers

    partial void OnDescriptionChanged(string value) => ValidateForm();
    partial void OnAmountChanged(decimal value) => ValidateForm();
    partial void OnSelectedCategoryChanged(CategoryDto? value) => ValidateForm();

    #endregion
}
