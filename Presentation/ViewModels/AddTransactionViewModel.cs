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
/// ViewModel used to add or edit transactions.
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

        Title = "New Transaction";
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

    #region Calculated Properties

    /// <summary>
    /// Text displayed on the primary button.
    /// </summary>
    public string SaveButtonText => IsEditMode ? "Update" : "Save";

    /// <summary>
    /// Formatted amount for display.
    /// </summary>
    public string FormattedAmount => Amount > 0 ? $"${Amount:N2}" : "$0.00";

    /// <summary>
    /// Indicates whether it is an expense (used to adjust UI colors).
    /// </summary>
    public bool IsExpense => TransactionType == TransactionType.Expense;

    /// <summary>
    /// Color associated with the transaction type.
    /// </summary>
    public string TypeColor => IsExpense ? "#F44336" : "#4CAF50";

    #endregion

    #region Commands

    /// <summary>
    /// Loads the available categories.
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

            // Select the first category by default when none is selected
            if (SelectedCategory == null && Categories.Any())
            {
                SelectedCategory = Categories.First();
            }
        });
    }

    /// <summary>
    /// Changes the transaction type.
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
    /// Saves the transaction.
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
    /// Cancels the operation.
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
    /// Clears the form.
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

    #region Public Methods

    /// <summary>
    /// Configures the ViewModel to edit an existing transaction.
    /// </summary>
    public void SetEditMode(TransactionDto transaction)
    {
        IsEditMode = true;
        TransactionId = transaction.Id;
        Title = "Edit Transaction";

        // Load the transaction data
        Description = transaction.Description;
        Amount = transaction.Amount;
        Currency = transaction.Currency;
        TransactionType = transaction.Type;
        TransactionDate = transaction.Date;
        Notes = transaction.Notes ?? string.Empty;
        Location = transaction.Location ?? string.Empty;
        IsRecurring = transaction.IsRecurring;

        // Select the matching category
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == transaction.CategoryId);

        ValidateForm();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a new transaction.
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
            DialogService?.ShowToast("Transaction created successfully");
            if (NavigationService != null)
            {
                await NavigationService.NavigateBackAsync();
            }
        }
        else
        {
            if (DialogService != null)
            {
                await DialogService.ShowErrorAsync("Failed to create the transaction");
            }
            await ShowValidationErrorsAsync(errors);
        }
    }

    /// <summary>
    /// Updates an existing transaction.
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
            DialogService?.ShowToast("Transaction updated successfully");
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
    /// Validates the entire form.
    /// </summary>
    private bool ValidateForm()
    {
        ClearValidationErrors();

        // Validate description
        if (string.IsNullOrWhiteSpace(Description))
            AddValidationError("The description is required");
        else if (Description.Trim().Length < 3)
            AddValidationError("The description must have at least 3 characters");

        // Validate amount
        if (Amount <= 0)
            AddValidationError("The amount must be greater than zero");
        else if (Amount > 999999999)
            AddValidationError("The amount is too large");

        // Validate category
        if (SelectedCategory == null)
            AddValidationError("A category must be selected");

        // Validate date
        if (TransactionDate > DateTime.Now.AddDays(1))
            AddValidationError("The date cannot be in the future");

        // Validate notes (optional)
        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
            AddValidationError("Notes cannot exceed 500 characters");

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
            AddValidationError("An unexpected error occurred");
            errors = ValidationErrors.ToList();
        }

        foreach (var error in errors)
            AddValidationError(error);

        if (DialogService != null)
        {
            await DialogService.ShowErrorAsync("Please fix the highlighted errors");
        }
    }

    #endregion

    #region Event Handlers

    partial void OnDescriptionChanged(string value) => ValidateForm();
    partial void OnAmountChanged(decimal value) => ValidateForm();
    partial void OnSelectedCategoryChanged(CategoryDto? value) => ValidateForm();

    #endregion
}
