using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Messages;
using System.Collections.ObjectModel;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel para la pantalla principal de lista de transacciones
/// </summary>
public partial class TransactionListViewModel : BaseViewModel
{
    private readonly TransactionAppService _transactionService;

    public TransactionListViewModel(TransactionAppService transactionService)
    {
        _transactionService = transactionService;
        Title = "Mis Transacciones";

        // Inicializar colecciones
        Transactions = new ObservableCollection<TransactionDto>();
        FilteredTransactions = new ObservableCollection<TransactionDto>();

        // Cargar datos iniciales
        _ = LoadDataAsync();
    }

    #region Propiedades Observables

    /// <summary>
    /// Todas las transacciones
    /// </summary>
    public ObservableCollection<TransactionDto> Transactions { get; }

    /// <summary>
    /// Transacciones filtradas para mostrar
    /// </summary>
    public ObservableCollection<TransactionDto> FilteredTransactions { get; }

    /// <summary>
    /// Filtro de texto para buscar
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Filtro por tipo de transacción
    /// </summary>
    [ObservableProperty]
    private TransactionType? _selectedType = null;

    /// <summary>
    /// Balance total calculado
    /// </summary>
    [ObservableProperty]
    private decimal _totalBalance;

    /// <summary>
    /// Total de ingresos del período
    /// </summary>
    [ObservableProperty]
    private decimal _totalIncome;

    /// <summary>
    /// Total de gastos del período
    /// </summary>
    [ObservableProperty]
    private decimal _totalExpenses;

    /// <summary>
    /// Indica si hay transacciones para mostrar
    /// </summary>
    [ObservableProperty]
    private bool _hasTransactions;

    /// <summary>
    /// Mensaje cuando no hay transacciones
    /// </summary>
    [ObservableProperty]
    private string _emptyMessage = "No hay transacciones aún.\n¡Agrega tu primera transacción!";

    #endregion

    #region Propiedades Calculadas

    /// <summary>
    /// Balance formateado para mostrar en UI
    /// </summary>
    public string FormattedBalance => TotalBalance >= 0
        ? $"+${TotalBalance:N2}"
        : $"-${Math.Abs(TotalBalance):N2}";

    /// <summary>
    /// Color del balance según si es positivo o negativo
    /// </summary>
    public string BalanceColor => TotalBalance >= 0 ? "#4CAF50" : "#F44336";

    /// <summary>
    /// Ingresos formateados
    /// </summary>
    public string FormattedIncome => $"${TotalIncome:N2}";

    /// <summary>
    /// Gastos formateados
    /// </summary>
    public string FormattedExpenses => $"${TotalExpenses:N2}";

    #endregion

    #region Comandos

    /// <summary>
    /// Carga todas las transacciones
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            // Actualizar colección principal
            Transactions.Clear();
            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }

            // Aplicar filtros
            ApplyFilters();

            // Calcular totales
            CalculateTotals();

            // Actualizar estado
            HasTransactions = Transactions.Any();

            // Notificar cambios en propiedades calculadas
            OnPropertyChanged(nameof(FormattedBalance));
            OnPropertyChanged(nameof(BalanceColor));
            OnPropertyChanged(nameof(FormattedIncome));
            OnPropertyChanged(nameof(FormattedExpenses));
        });
    }

    /// <summary>
    /// Buscar transacciones por texto
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        ApplyFilters();
    }

    /// <summary>
    /// Filtrar por tipo de transacción
    /// </summary>
    [RelayCommand]
    private void FilterByType(TransactionType? type)
    {
        SelectedType = SelectedType == type ? null : type;
        ApplyFilters();
    }

    /// <summary>
    /// Limpiar todos los filtros
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedType = null;
        ApplyFilters();
    }

    /// <summary>
    /// Navegar a agregar transacción
    /// </summary>
    [RelayCommand]
    private void NavigateToAddTransaction()
    {
        // Notificar a la UI para navegar
        WeakReferenceMessenger.Default.Send(new NavigateToAddTransactionMessage());
    }

    /// <summary>
    /// Editar una transacción
    /// </summary>
    [RelayCommand]
    private void EditTransaction(TransactionDto transaction)
    {
        if (transaction == null) return;

        WeakReferenceMessenger.Default.Send(new NavigateToEditTransactionMessage(transaction));

    }

    /// <summary>
    /// Eliminar una transacción
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
                // Remover de la colección local
                Transactions.Remove(transaction);
                ApplyFilters();
                CalculateTotals();

                // Mostrar mensaje de éxito
                WeakReferenceMessenger.Default.Send(new ShowMessageMessage("Transacción eliminada correctamente"));
            }
            else
            {
                // Mostrar errores
                var errorMessage = string.Join("\n", result.Errors);
                WeakReferenceMessenger.Default.Send(new ShowErrorMessage("Ocurrió un error"));
            }
        });
    }

    /// <summary>
    /// Refresh manual
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

    #region Métodos Privados

    /// <summary>
    /// Aplica los filtros actuales a la lista de transacciones
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = Transactions.AsEnumerable();

        // Filtro por texto
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(t =>
                t.Description.ToLowerInvariant().Contains(search) ||
                t.CategoryName.ToLowerInvariant().Contains(search) ||
                t.Notes?.ToLowerInvariant().Contains(search) == true);
        }

        // Filtro por tipo
        if (SelectedType.HasValue)
        {
            filtered = filtered.Where(t => t.Type == SelectedType.Value);
        }

        // Actualizar colección filtrada
        FilteredTransactions.Clear();
        foreach (var transaction in filtered.OrderByDescending(t => t.Date))
        {
            FilteredTransactions.Add(transaction);
        }

        // Actualizar mensaje vacío
        EmptyMessage = GetEmptyMessage();
    }

    /// <summary>
    /// Calcula los totales de ingresos, gastos y balance
    /// </summary>
    private void CalculateTotals()
    {
        TotalIncome = Transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        TotalExpenses = Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        TotalBalance = TotalIncome - TotalExpenses;
    }

    /// <summary>
    /// Obtiene el mensaje apropiado cuando no hay transacciones
    /// </summary>
    private string GetEmptyMessage()
    {
        if (!HasTransactions)
        {
            return "No hay transacciones aún.\n¡Agrega tu primera transacción!";
        }

        if (!string.IsNullOrWhiteSpace(SearchText) || SelectedType.HasValue)
        {
            return "No se encontraron transacciones\ncon los filtros aplicados.";
        }

        return "No hay transacciones para mostrar.";
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Se llama cuando cambia el texto de búsqueda
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        Task.Delay(300).ContinueWith(_ => Search());
    }

    #endregion
}