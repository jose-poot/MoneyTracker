using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Motion.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.Chip;
using Google.Android.Material.FloatingActionButton;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Adapters;
using MoneyTracker.Presentation.ViewModels;
using System;
using System.Linq;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace MoneyTracker.Presentation.Fragments
{
    /// <summary>
    /// Fragment que muestra la lista de transacciones con funcionalidad de filtros
    /// </summary>
    public class TransactionListFragment : Fragment
    {
        private TransactionListViewModel? _viewModel;
        private TransactionAdapter? _adapter;

        // Controles de UI
        private RecyclerView? _recyclerView;
        private SwipeRefreshLayout? _swipeRefresh;
        private TextView? _balanceText;
        private TextView? _incomeText;
        private TextView? _expensesText;
        private EditText? _searchEditText;
        private LinearLayout? _filterChipGroup;
        private Button? _allChip;
        private Button? _incomeChip;
        private Button? _expenseChip;
        private FloatingActionButton? _fabAdd;
        private LinearLayout? _emptyStateLayout;
        private TextView? _emptyMessageText;

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_transaction_list, container, false);

            InitializeViews(view);
            SetupRecyclerView();
            SetupEventHandlers();

            return view;
        }

        public override void OnViewCreated(View view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Obtener ViewModel del DI container
            _viewModel = MoneyTrackerApplication.GetService<TransactionListViewModel>();

            BindViewModel();
        }

        /// <summary>
        /// Inicializa todas las vistas del layout
        /// </summary>
        private void InitializeViews(View view)
        {
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_transactions);
            _swipeRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh);
            _balanceText = view.FindViewById<TextView>(Resource.Id.text_balance);
            _incomeText = view.FindViewById<TextView>(Resource.Id.text_income);
            _expensesText = view.FindViewById<TextView>(Resource.Id.text_expenses);
            _searchEditText = view.FindViewById<EditText>(Resource.Id.edit_search);
            _filterChipGroup = view.FindViewById<LinearLayout>(Resource.Id.chip_group_filters);
            _allChip = view.FindViewById<Button>(Resource.Id.chip_all);
            _incomeChip = view.FindViewById<Button>(Resource.Id.chip_income);
            _expenseChip = view.FindViewById<Button>(Resource.Id.chip_expense);
            _fabAdd = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add);
            _emptyStateLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_empty_state);
            _emptyMessageText = view.FindViewById<TextView>(Resource.Id.text_empty_message);
        }

        /// <summary>
        /// Configura el RecyclerView con su adapter
        /// </summary>
        private void SetupRecyclerView()
        {
            if (_recyclerView != null)
            {
                _recyclerView.SetLayoutManager(new LinearLayoutManager(Context));

                _adapter = new TransactionAdapter();
                _recyclerView.SetAdapter(_adapter);

                // Configurar callbacks del adapter
                _adapter.ItemClick += OnTransactionClick;
                _adapter.EditClick += OnTransactionEdit;
                _adapter.DeleteClick += OnTransactionDelete;
            }
        }

  

        /// <summary>
        /// Conecta el ViewModel con la UI
        /// </summary>
        private void BindViewModel()
        {
            if (_viewModel == null) return;

            // Observar cambios en las transacciones
            _viewModel.FilteredTransactions.CollectionChanged += (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    _adapter?.UpdateTransactions(_viewModel.FilteredTransactions.ToList());
                    UpdateEmptyState();
                });
            };

            // Observar cambios en propiedades
            _viewModel.PropertyChanged += (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(TransactionListViewModel.FormattedBalance):
                            UpdateBalanceDisplay();
                            break;
                        case nameof(TransactionListViewModel.FormattedIncome):
                            if (_incomeText != null)
                                _incomeText.Text = _viewModel.FormattedIncome;
                            break;
                        case nameof(TransactionListViewModel.FormattedExpenses):
                            if (_expensesText != null)
                                _expensesText.Text = _viewModel.FormattedExpenses;
                            break;
                        case nameof(TransactionListViewModel.IsRefreshing):
                            if (_swipeRefresh != null)
                                _swipeRefresh.Refreshing = _viewModel.IsRefreshing;
                            break;
                        case nameof(TransactionListViewModel.EmptyMessage):
                            UpdateEmptyState();
                            break;
                    }
                });
            };

            // Configuración inicial
            UpdateBalanceDisplay();
            UpdateEmptyState();
        }

        /// <summary>
        /// Actualiza la visualización del balance con colores
        /// </summary>
        private void UpdateBalanceDisplay()
        {
            if (_balanceText == null || _viewModel == null) return;

            _balanceText.Text = _viewModel.FormattedBalance;

            // Cambiar color según balance
            var color = Android.Graphics.Color.ParseColor(_viewModel.BalanceColor);
            _balanceText.SetTextColor(color);
        }

        /// <summary>
        /// Actualiza el estado vacío de la lista
        /// </summary>
        private void UpdateEmptyState()
        {
            if (_viewModel == null || _emptyStateLayout == null || _recyclerView == null)
                return;

            var isEmpty = !_viewModel.FilteredTransactions.Any();

            _emptyStateLayout.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
            _recyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;

            if (_emptyMessageText != null)
            {
                _emptyMessageText.Text = _viewModel.EmptyMessage;
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en los chips de filtro
        /// </summary>
        private void SetupFilterButtons()
        {
            _allChip?.SetOnClickListener(new FilterClickListener(_viewModel, null));
            _incomeChip?.SetOnClickListener(new FilterClickListener(_viewModel, TransactionType.Income));
            _expenseChip?.SetOnClickListener(new FilterClickListener(_viewModel, TransactionType.Expense));
        }

        private class FilterClickListener : Java.Lang.Object, View.IOnClickListener
        {
            private readonly TransactionListViewModel? _viewModel;
            private readonly TransactionType? _filterType;

            public FilterClickListener(TransactionListViewModel? viewModel, TransactionType? filterType)
            {
                _viewModel = viewModel;
                _filterType = filterType;
            }

            public void OnClick(View? v)
            {
                _viewModel?.FilterByTypeCommand.Execute(_filterType);
            }
        }
        private void SetupEventHandlers()
        {
            // FAB para agregar transacción
            if (_fabAdd != null)
            {
                _fabAdd.Click += (s, e) => _viewModel?.NavigateToAddTransactionCommand.Execute(null);
            }

            // SwipeRefresh
            if (_swipeRefresh != null)
            {
                _swipeRefresh.Refresh += async (s, e) =>
                {
                    await (_viewModel?.RefreshCommand.ExecuteAsync(null) ?? Task.CompletedTask);
                };
            }

            // Búsqueda
            if (_searchEditText != null)
            {
                _searchEditText.TextChanged += (s, e) =>
                {
                    if (_viewModel != null)
                    {
                        _viewModel.SearchText = e?.Text?.ToString() ?? string.Empty;
                    }
                };
            }

            // ✅ FILTROS COMO BOTONES SIMPLES
            if (_allChip != null)
            {
                _allChip.Click += (s, e) => _viewModel?.FilterByTypeCommand.Execute(null);
            }

            if (_incomeChip != null)
            {
                _incomeChip.Click += (s, e) => _viewModel?.FilterByTypeCommand.Execute(TransactionType.Income);
            }

            if (_expenseChip != null)
            {
                _expenseChip.Click += (s, e) => _viewModel?.FilterByTypeCommand.Execute(TransactionType.Expense);
            }
        }
        /// <summary>
        /// Maneja el click en una transacción (ver detalles)
        /// </summary>
        private void OnTransactionClick(TransactionDto transaction)
        {
            // Por ahora, mismo comportamiento que editar
            OnTransactionEdit(transaction);
        }

        /// <summary>
        /// Maneja el click en editar transacción
        /// </summary>
        private void OnTransactionEdit(TransactionDto transaction)
        {
            _viewModel?.EditTransactionCommand.Execute(transaction);
        }

        /// <summary>
        /// Maneja el click en eliminar transacción
        /// </summary>
        private void OnTransactionDelete(TransactionDto transaction)
        {
            // Mostrar confirmación antes de eliminar
            new AndroidX.AppCompat.App.AlertDialog.Builder(RequireContext())
                .SetTitle("Confirmar eliminación")
                .SetMessage($"¿Estás seguro de que quieres eliminar '{transaction.Description}'?")
                .SetPositiveButton("Eliminar", async (s, e) =>
                {
                    await (_viewModel?.DeleteTransactionCommand.ExecuteAsync(transaction) ?? Task.CompletedTask);
                })
                .SetNegativeButton("Cancelar", (s, e) => { })
                .Show();
        }

        /// <summary>
        /// Limpieza cuando se destruye el fragment
        /// </summary>
        public override void OnDestroyView()
        {
            // Desconectar adapter para evitar memory leaks
            if (_adapter != null)
            {
                _adapter.ItemClick -= OnTransactionClick;
                _adapter.EditClick -= OnTransactionEdit;
                _adapter.DeleteClick -= OnTransactionDelete;
            }

            base.OnDestroyView();
        }
    }
}