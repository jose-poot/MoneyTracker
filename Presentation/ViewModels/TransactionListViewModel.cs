using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Collections;
using MoneyTracker.Presentation.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel for the main transaction list screen.
/// </summary>
public partial class TransactionListViewModel : BaseViewModel
{
    private readonly TransactionAppService _transactionService;
    private readonly CategoryAppService _categoryService;
    private readonly List<TransactionDto> _allTransactions = new();
    private readonly List<TransactionDto> _currentFilteredTransactions = new();
    private readonly List<CategoryDto> _categories = new();
    private DateTime? _currentRangeStart;
    private DateTime? _currentRangeEnd;
    private bool _suspendFilterUpdates;

    public TransactionListViewModel(
        TransactionAppService transactionService,
        CategoryAppService categoryService,
        IDialogService dialogService,
        INavigationService navigationService,
        ICacheService? cacheService = null)
        : base(dialogService, navigationService, cacheService)

    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        Title = "My Transactions";

        FilteredTransactions = new VirtualizedObservableCollection<TransactionDto>(pageSize: 25);
        FilteredTransactions.SetSortComparer((x, y) => DateTime.Compare(y.Date, x.Date));

        // Load initial data
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Available date filter options.
    /// </summary>
    public enum DateFilterOption
    {
        AllTime,
        CurrentMonth,
        Last30Days
    }

    #region Observable Properties

    /// <summary>
    /// Transactions filtered for display.
    /// </summary>
    public VirtualizedObservableCollection<TransactionDto> FilteredTransactions { get; }

    /// <summary>
    /// Transactions visible on the current page.
    /// </summary>
    public IReadOnlyList<TransactionDto> VisibleTransactions => FilteredTransactions.VisibleItems;

    /// <summary>
    /// Indicates whether more items can be paged in.
    /// </summary>
    public bool HasMoreTransactions => FilteredTransactions.HasMorePages;

    /// <summary>
    /// Text filter used for search.
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Filter by transaction type.
    /// </summary>
    [ObservableProperty]
    private TransactionType? _selectedType = null;

    /// <summary>
    /// Calculated total balance.
    /// </summary>
    [ObservableProperty]
    private decimal _totalBalance;

    /// <summary>
    /// Total income for the selected period.
    /// </summary>
    [ObservableProperty]
    private decimal _totalIncome;

    /// <summary>
    /// Total expenses for the selected period.
    /// </summary>
    [ObservableProperty]
    private decimal _totalExpenses;

    /// <summary>
    /// Indicates if there are transactions to display.
    /// </summary>
    [ObservableProperty]
    private bool _hasTransactions;

    /// <summary>
    /// Message displayed when there are no transactions.
    /// </summary>
    [ObservableProperty]
    private string _emptyMessage = "No transactions yet.\nAdd your first transaction!";

    /// <summary>
    /// Selected category used for filtering.
    /// </summary>
    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    /// <summary>
    /// Determines whether only recurring transactions are shown.
    /// </summary>
    [ObservableProperty]
    private bool _showOnlyRecurring;

    /// <summary>
    /// Selected date filter option.
    /// </summary>
    [ObservableProperty]
    private DateFilterOption _selectedDateFilter = DateFilterOption.CurrentMonth;

    /// <summary>
    /// Readable description of the applied date range.
    /// </summary>
    [ObservableProperty]
    private string _dateRangeDescription = string.Empty;

    /// <summary>
    /// Text containing quick recommendations or statistics.
    /// </summary>
    [ObservableProperty]
    private string _spendingInsights = string.Empty;

    #endregion

    #region Calculated Properties

    /// <summary>
    /// Balance formatted for the UI.
    /// </summary>
    public string FormattedBalance => TotalBalance >= 0
        ? $"+${TotalBalance:N2}"
        : $"-${Math.Abs(TotalBalance):N2}";

    /// <summary>
    /// Balance color depending on whether it is positive or negative.
    /// </summary>
    public string BalanceColor => TotalBalance >= 0 ? "#4CAF50" : "#F44336";

    /// <summary>
    /// Formatted income.
    /// </summary>
    public string FormattedIncome => $"${TotalIncome:N2}";

    /// <summary>
    /// Formatted expenses.
    /// </summary>
    public string FormattedExpenses => $"${TotalExpenses:N2}";

    /// <summary>
    /// Categories available for filtering.
    /// </summary>
    public IReadOnlyList<CategoryDto> Categories => _categories;

    #endregion

    #region Commands

    /// <summary>
    /// Loads every transaction.
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var loadCategoriesTask = LoadCategoriesAsync();
            var transactions = await _transactionService.GetAllTransactionsAsync();
            await loadCategoriesTask;

            _allTransactions.Clear();
            if (transactions != null)
            {
                _allTransactions.AddRange(transactions);
            }

            ApplyFilters();
        });
    }

    /// <summary>
    /// Searches transactions by text.
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        ApplyFilters();
    }

    /// <summary>
    /// Filters by transaction type.
    /// </summary>
    [RelayCommand]
    private void FilterByType(TransactionType? type)
    {
        SelectedType = SelectedType == type ? null : type;
        ApplyFilters();
    }

    /// <summary>
    /// Changes the active date filter.
    /// </summary>
    [RelayCommand]
    private void SetDateFilter(DateFilterOption option)
    {
        SelectedDateFilter = option;
    }

    /// <summary>
    /// Clears every filter.
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        _suspendFilterUpdates = true;
        try
        {
            SearchText = string.Empty;
            SelectedType = null;
            SelectedCategory = null;
            ShowOnlyRecurring = false;
            SelectedDateFilter = DateFilterOption.AllTime;
        }
        finally
        {
            _suspendFilterUpdates = false;
        }

        ApplyFilters();
    }

    /// <summary>
    /// Navigates to the screen for adding a transaction.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToAddTransaction()
    {

        _ = NavigationService?.NavigateToAsync("AddTransaction");
        if (NavigationService == null)
        {
            return;
        }

        await NavigationService.NavigateToAsync("AddTransaction");
    }

    /// <summary>
    /// Edits a transaction.
    /// </summary>
    [RelayCommand]
    private async Task EditTransaction(TransactionDto transaction)
    {
        if (transaction == null) return;

        _ = NavigationService?.NavigateToAsync("EditTransaction", transaction);

        if (NavigationService == null)
        {
            return;
        }


        await NavigationService.NavigateToAsync("EditTransaction", transaction);
    }

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    [RelayCommand]
    private async Task DeleteTransactionAsync(TransactionDto transaction)
    {
        if (transaction == null) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _transactionService.DeleteTransactionAsync(transaction.Id);

            if (result.Success)
            {
                if (_allTransactions.Remove(transaction))
                {
                    ApplyFilters();
                    CalculateTotals();
                }

                HasTransactions = _allTransactions.Any();
                EmptyMessage = GetEmptyMessage();


                DialogService?.ShowToast("Transaction deleted successfully");
                await ShowMessageAsync("Transaction deleted successfully");

            }
            else
            {
                // Display errors
                var errorMessage = string.Join("\n", result.Errors);

                _ = DialogService?.ShowErrorAsync(string.IsNullOrWhiteSpace(errorMessage)
                    ? "An error occurred"
                    : errorMessage);

                if (DialogService != null)
                {
                    var message = string.IsNullOrWhiteSpace(errorMessage) ? "An error occurred" : errorMessage;
                    await DialogService.ShowErrorAsync(message);
                }

            }
        });
    }

    /// <summary>
    /// Attempts to load the next page of results.
    /// </summary>
    public bool TryLoadMoreTransactions()
    {
        var loaded = FilteredTransactions.LoadNextPage();
        if (loaded)
        {
            OnPropertyChanged(nameof(VisibleTransactions));
            OnPropertyChanged(nameof(HasMoreTransactions));
        }

        return loaded;
    }

    /// <summary>
    /// Manual refresh implementation.
    /// </summary>
    protected override async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadDataAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Applies the current filters to the transaction list.
    /// </summary>
    private void ApplyFilters()
    {
        IEnumerable<TransactionDto> filtered = _allTransactions;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(t =>
                t.Description.ToLowerInvariant().Contains(search) ||
                t.CategoryName.ToLowerInvariant().Contains(search) ||
                t.Notes?.ToLowerInvariant().Contains(search) == true);
        }

        if (SelectedType.HasValue)
        {
            filtered = filtered.Where(t => t.Type == SelectedType.Value);
        }

        if (SelectedCategory != null)
        {
            filtered = filtered.Where(t => t.CategoryId == SelectedCategory.Id);
        }

        if (ShowOnlyRecurring)
        {
            filtered = filtered.Where(t => t.IsRecurring);
        }

        var (startDate, endDate) = GetDateRange();
        _currentRangeStart = startDate;
        _currentRangeEnd = endDate;

        if (startDate.HasValue)
        {
            filtered = filtered.Where(t => t.Date.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            filtered = filtered.Where(t => t.Date.Date <= endDate.Value.Date);
        }

        var filteredList = filtered.ToList();
        _currentFilteredTransactions.Clear();
        _currentFilteredTransactions.AddRange(filteredList);

        var previousPage = FilteredTransactions.CurrentPage;
        FilteredTransactions.ReplaceAll(filteredList);

        var maxPageIndex = Math.Max(0, (int)Math.Ceiling(FilteredTransactions.TotalCount / (double)FilteredTransactions.PageSize) - 1);
        var targetPage = Math.Min(previousPage, maxPageIndex);
        if (targetPage > 0)
        {
            FilteredTransactions.GoToPage(targetPage);
        }

        CalculateTotals();
        EmptyMessage = GetEmptyMessage();
        HasTransactions = _allTransactions.Any();
        UpdateDateRangeDescription();
        UpdateInsights();
        OnPropertyChanged(nameof(FormattedBalance));
        OnPropertyChanged(nameof(BalanceColor));
        OnPropertyChanged(nameof(FormattedIncome));
        OnPropertyChanged(nameof(FormattedExpenses));
        OnPropertyChanged(nameof(VisibleTransactions));
        OnPropertyChanged(nameof(HasMoreTransactions));
    }

    /// <summary>
    /// Calculates totals for income, expenses, and balance.
    /// </summary>
    private void CalculateTotals()
    {
        TotalIncome = _currentFilteredTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        TotalExpenses = _currentFilteredTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        TotalBalance = TotalIncome - TotalExpenses;
    }

    /// <summary>
    /// Gets the appropriate empty-state message.
    /// </summary>
    private string GetEmptyMessage()
    {
        if (!_allTransactions.Any())
        {
            return "No transactions yet.\nAdd your first transaction!";
        }

        if (!_currentFilteredTransactions.Any())
        {
            return "No transactions found with the applied filters.";
        }

        return "There are no transactions to display.";
    }

    /// <summary>
    /// Gets the active date range based on the selected option.
    /// </summary>
    private (DateTime? Start, DateTime? End) GetDateRange()
    {
        var today = DateTime.Today;
        return SelectedDateFilter switch
        {
            DateFilterOption.CurrentMonth =>
                (new DateTime(today.Year, today.Month, 1),
                 new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))),
            DateFilterOption.Last30Days =>
                (today.AddDays(-29), today),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Updates the active date range description shown in the UI.
    /// </summary>
    private void UpdateDateRangeDescription()
    {
        var (start, end) = (_currentRangeStart, _currentRangeEnd);

        if (!start.HasValue && !end.HasValue)
        {
            DateRangeDescription = "Showing: All dates";
            return;
        }

        if (start.HasValue && end.HasValue)
        {
            DateRangeDescription = $"Showing: {start.Value:dd MMM yyyy} - {end.Value:dd MMM yyyy}";
            return;
        }

        if (start.HasValue)
        {
            DateRangeDescription = $"Showing: From {start.Value:dd MMM yyyy}";
            return;
        }

        DateRangeDescription = $"Showing: Until {end!.Value:dd MMM yyyy}";
    }

    /// <summary>
    /// Calculates quick metrics to provide recommendations.
    /// </summary>
    private void UpdateInsights()
    {
        if (!_currentFilteredTransactions.Any())
        {
            SpendingInsights = "Add transactions to see personalized statistics.";
            return;
        }

        var expenses = _currentFilteredTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .ToList();

        var startDate = _currentRangeStart ?? _currentFilteredTransactions.Min(t => t.Date).Date;
        var endDate = _currentRangeEnd ?? _currentFilteredTransactions.Max(t => t.Date).Date;
        var totalDays = Math.Max(1, (endDate - startDate).Days + 1);

        var totalExpenses = expenses.Sum(t => t.Amount);
        var dailyAverage = totalExpenses / totalDays;
        var recurringCount = _currentFilteredTransactions.Count(t => t.IsRecurring);

        var topCategory = expenses
            .GroupBy(t => t.CategoryName)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Total)
            .FirstOrDefault();

        if (topCategory == null || topCategory.Total <= 0)
        {
            SpendingInsights = $"Daily spending average: ${dailyAverage:N2}. Active recurring transactions: {recurringCount}.";
            return;
        }

        SpendingInsights = $"Top category: {topCategory.Category} (${topCategory.Total:N2}). Daily spending average: ${dailyAverage:N2}. Recurring transactions: {recurringCount}.";
    }

    /// <summary>
    /// Loads the categories available for filtering.
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();

            _categories.Clear();
            if (categories != null)
            {
                _categories.AddRange(categories.OrderBy(c => c.Name));
            }

            OnPropertyChanged(nameof(Categories));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Invoked when the search text changes.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        Task.Delay(300).ContinueWith(_ => Search());
    }

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        if (!_suspendFilterUpdates)
        {
            ApplyFilters();
        }
    }

    partial void OnShowOnlyRecurringChanged(bool value)
    {
        if (!_suspendFilterUpdates)
        {
            ApplyFilters();
        }
    }

    partial void OnSelectedDateFilterChanged(DateFilterOption value)
    {
        if (!_suspendFilterUpdates)
        {
            ApplyFilters();
        }
    }

    #endregion
}