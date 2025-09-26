using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Adapters;
using MoneyTracker.Presentation.Extensions;
using MoneyTracker.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace MoneyTracker.Presentation.Fragments
{
    /// <summary>
    /// Fragment that displays the transaction list with filtering capabilities.
    /// </summary>
    public class TransactionListFragment : Fragment
    {
        private TransactionListViewModel? _viewModel;
        private TransactionAdapter? _adapter;
        private CompositeDisposable? _subscriptions;
        private LinearLayoutManager? _layoutManager;
        private EndlessScrollListener? _scrollListener;

        // UI controls
        private RecyclerView? _recyclerView;
        private SwipeRefreshLayout? _swipeRefresh;
        private TextView? _balanceText;
        private TextView? _incomeText;
        private TextView? _expensesText;
        private EditText? _searchEditText;
        private Spinner? _categorySpinner;
        private CheckBox? _recurringCheckBox;
        private TextView? _dateRangeText;
        private Button? _dateAllButton;
        private Button? _dateCurrentMonthButton;
        private Button? _dateLast30Button;
        private TextView? _insightsText;
        private LinearLayout? _filterChipGroup;
        private Button? _allChip;
        private Button? _incomeChip;
        private Button? _expenseChip;
        private FloatingActionButton? _fabAdd;
        private LinearLayout? _emptyStateLayout;
        private TextView? _emptyMessageText;
        private ArrayAdapter<string>? _categoryAdapter;
        private bool _suppressCategoryEvent;
        private bool _suppressRecurringEvent;

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

            // Resolve the ViewModel from the DI container
            _viewModel = MoneyTrackerApplication.GetService<TransactionListViewModel>();

            BindViewModel();
        }

        /// <summary>
        /// Initializes every view in the layout.
        /// </summary>
        private void InitializeViews(View view)
        {
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_transactions);
            _swipeRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh);
            _balanceText = view.FindViewById<TextView>(Resource.Id.text_balance);
            _incomeText = view.FindViewById<TextView>(Resource.Id.text_income);
            _expensesText = view.FindViewById<TextView>(Resource.Id.text_expenses);
            _searchEditText = view.FindViewById<EditText>(Resource.Id.edit_search);
            _categorySpinner = view.FindViewById<Spinner>(Resource.Id.spinner_category);
            _recurringCheckBox = view.FindViewById<CheckBox>(Resource.Id.checkbox_recurring);
            _dateRangeText = view.FindViewById<TextView>(Resource.Id.text_date_range);
            _dateAllButton = view.FindViewById<Button>(Resource.Id.button_date_all);
            _dateCurrentMonthButton = view.FindViewById<Button>(Resource.Id.button_date_current_month);
            _dateLast30Button = view.FindViewById<Button>(Resource.Id.button_date_last_30);
            _insightsText = view.FindViewById<TextView>(Resource.Id.text_insights);
            _filterChipGroup = view.FindViewById<LinearLayout>(Resource.Id.chip_group_filters);
            _allChip = view.FindViewById<Button>(Resource.Id.chip_all);
            _incomeChip = view.FindViewById<Button>(Resource.Id.chip_income);
            _expenseChip = view.FindViewById<Button>(Resource.Id.chip_expense);
            _fabAdd = view.FindViewById<FloatingActionButton>(Resource.Id.fab_add);
            _emptyStateLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_empty_state);
            _emptyMessageText = view.FindViewById<TextView>(Resource.Id.text_empty_message);
        }

        /// <summary>
        /// Configures the RecyclerView with its adapter.
        /// </summary>
        private void SetupRecyclerView()
        {
            if (_recyclerView != null)
            {
                _layoutManager = new LinearLayoutManager(Context);
                _recyclerView.SetLayoutManager(_layoutManager);
                _recyclerView.SetHasFixedSize(true);

                _adapter = new TransactionAdapter();
                _recyclerView.SetAdapter(_adapter);

                _scrollListener = new EndlessScrollListener(
                    _layoutManager,
                    () => _viewModel?.HasMoreTransactions == true,
                    OnLoadMoreRequested);

                _recyclerView.AddOnScrollListener(_scrollListener);

                // Configure adapter callbacks
                _adapter.ItemClick += OnTransactionClick;
                _adapter.EditClick += OnTransactionEdit;
                _adapter.DeleteClick += OnTransactionDelete;
            }
        }

        private void OnLoadMoreRequested()
        {
            if (_viewModel == null)
            {
                _scrollListener?.SetLoading(false);
                return;
            }

            if (!_viewModel.TryLoadMoreTransactions())
            {
                _scrollListener?.SetLoading(false);
            }
        }

  

        /// <summary>
        /// Connects the ViewModel with the UI.
        /// </summary>
        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _subscriptions?.Dispose();
            _subscriptions = new CompositeDisposable();

            NotifyCollectionChangedEventHandler collectionChangedHandler = (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    UpdateTransactionList();
                });
            };

            _viewModel.FilteredTransactions.CollectionChanged += collectionChangedHandler;
            _subscriptions.Add(new ActionDisposable(() =>
                _viewModel.FilteredTransactions.CollectionChanged -= collectionChangedHandler));

            PropertyChangedEventHandler propertyChangedHandler = (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(TransactionListViewModel.FormattedBalance) || e.PropertyName == nameof(TransactionListViewModel.BalanceColor))
                    {
                        UpdateBalanceDisplay();
                    }

                    switch (e.PropertyName)
                    {
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
                        case nameof(TransactionListViewModel.VisibleTransactions):
                            UpdateTransactionList();
                            break;
                        case nameof(TransactionListViewModel.Categories):
                            PopulateCategorySpinner();
                            break;
                        case nameof(TransactionListViewModel.SelectedCategory):
                            UpdateCategorySelection();
                            break;
                        case nameof(TransactionListViewModel.ShowOnlyRecurring):
                            UpdateRecurringToggle();
                            break;
                        case nameof(TransactionListViewModel.SelectedDateFilter):
                            UpdateDateFilterButtons();
                            break;
                        case nameof(TransactionListViewModel.DateRangeDescription):
                            UpdateDateRangeText();
                            break;
                        case nameof(TransactionListViewModel.SpendingInsights):
                            UpdateInsights();
                            break;
                    }
                });
            };

            _viewModel.PropertyChanged += propertyChangedHandler;
            _subscriptions.Add(new ActionDisposable(() =>
                _viewModel.PropertyChanged -= propertyChangedHandler));

            UpdateBalanceDisplay();
            UpdateTransactionList();
            PopulateCategorySpinner();
            UpdateCategorySelection();
            UpdateRecurringToggle();
            UpdateDateFilterButtons();
            UpdateDateRangeText();
            UpdateInsights();
        }

        /// <summary>
        /// Updates the balance display with appropriate colors.
        /// </summary>
        private void UpdateBalanceDisplay()
        {
            if (_balanceText == null || _viewModel == null) return;

            _balanceText.Text = _viewModel.FormattedBalance;

            // Change color based on the balance
            var color = Android.Graphics.Color.ParseColor(_viewModel.BalanceColor);
            _balanceText.SetTextColor(color);
        }

        /// <summary>
        /// Updates the visible list in the RecyclerView.
        /// </summary>
        private void UpdateTransactionList()
        {
            if (_viewModel == null) return;

            var transactions = _viewModel.VisibleTransactions;
            _adapter?.UpdateTransactions(transactions);
            _scrollListener?.ResetState();
            UpdateEmptyState(transactions.Count);
        }

        /// <summary>
        /// Updates the empty state view.
        /// </summary>
        private void UpdateEmptyState(int? visibleCount = null)
        {
            if (_viewModel == null || _emptyStateLayout == null || _recyclerView == null)
                return;

            var count = visibleCount ?? _viewModel.VisibleTransactions.Count;
            var isEmpty = count == 0;

            _emptyStateLayout.Visibility = isEmpty ? ViewStates.Visible : ViewStates.Gone;
            _recyclerView.Visibility = isEmpty ? ViewStates.Gone : ViewStates.Visible;

            if (_emptyMessageText != null)
            {
                _emptyMessageText.Text = _viewModel.EmptyMessage;
            }
        }

        private void PopulateCategorySpinner()
        {
            if (_categorySpinner == null || _viewModel == null || Activity == null)
                return;

            var categories = _viewModel.Categories?.ToList() ?? new List<CategoryDto>();
            var items = new List<string> { GetString(Resource.String.transaction_filter_all) };
            items.AddRange(categories.Select(c => c.Name));

            _categoryAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleSpinnerItem, items);
            _categoryAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _categorySpinner.Adapter = _categoryAdapter;

            UpdateCategorySelection();
        }

        private void UpdateCategorySelection()
        {
            if (_categorySpinner == null || _viewModel == null)
                return;

            if (_categoryAdapter == null)
            {
                PopulateCategorySpinner();
                return;
            }

            var categories = _viewModel.Categories?.ToList() ?? new List<CategoryDto>();
            var selectedCategory = _viewModel.SelectedCategory;
            var index = 0;

            if (selectedCategory != null)
            {
                var catIndex = categories.FindIndex(c => c.Id == selectedCategory.Id);
                if (catIndex >= 0)
                {
                    index = catIndex + 1;
                }
            }

            _suppressCategoryEvent = true;
            _categorySpinner.SetSelection(index);
            _suppressCategoryEvent = false;
        }

        private void UpdateRecurringToggle()
        {
            if (_recurringCheckBox == null || _viewModel == null)
                return;

            _suppressRecurringEvent = true;
            _recurringCheckBox.Checked = _viewModel.ShowOnlyRecurring;
            _suppressRecurringEvent = false;
        }

        private void UpdateDateFilterButtons()
        {
            if (_viewModel == null)
                return;

            var option = _viewModel.SelectedDateFilter;

            if (_dateAllButton != null)
                _dateAllButton.Enabled = option != TransactionListViewModel.DateFilterOption.AllTime;

            if (_dateCurrentMonthButton != null)
                _dateCurrentMonthButton.Enabled = option != TransactionListViewModel.DateFilterOption.CurrentMonth;

            if (_dateLast30Button != null)
                _dateLast30Button.Enabled = option != TransactionListViewModel.DateFilterOption.Last30Days;
        }

        private void UpdateDateRangeText()
        {
            if (_dateRangeText == null || _viewModel == null)
                return;

            var description = string.IsNullOrWhiteSpace(_viewModel.DateRangeDescription)
                ? GetString(Resource.String.transaction_filter_all_dates)
                : _viewModel.DateRangeDescription;

            _dateRangeText.Text = description;
        }

        private void UpdateInsights()
        {
            if (_insightsText == null || _viewModel == null)
                return;

            var insights = string.IsNullOrWhiteSpace(_viewModel.SpendingInsights)
                ? GetString(Resource.String.transaction_insights_placeholder)
                : _viewModel.SpendingInsights;

            _insightsText.Text = insights;
        }

        private void SetupEventHandlers()
        {
            // FAB to add a transaction
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

            // Search
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

            if (_categorySpinner != null)
            {
                _categorySpinner.ItemSelected += OnCategorySelected;
            }

            if (_recurringCheckBox != null)
            {
                _recurringCheckBox.CheckedChange += OnRecurringCheckedChanged;
            }

            if (_dateAllButton != null)
            {
                _dateAllButton.Click += OnDateAllClicked;
            }

            if (_dateCurrentMonthButton != null)
            {
                _dateCurrentMonthButton.Click += OnDateCurrentMonthClicked;
            }

            if (_dateLast30Button != null)
            {
                _dateLast30Button.Click += OnDateLast30Clicked;
            }

            // ✅ Filters implemented as simple buttons
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
        /// Handles clicks on a transaction to show details.
        /// </summary>
        private void OnTransactionClick(TransactionDto transaction)
        {
            // For now it behaves the same as edit
            OnTransactionEdit(transaction);
        }

        /// <summary>
        /// Handles edit clicks on a transaction.
        /// </summary>
        private void OnTransactionEdit(TransactionDto transaction)
        {
            _viewModel?.EditTransactionCommand.Execute(transaction);
        }

        /// <summary>
        /// Handles delete clicks on a transaction.
        /// </summary>
        private void OnTransactionDelete(TransactionDto transaction)
        {
            // Show confirmation before deleting
            new AndroidX.AppCompat.App.AlertDialog.Builder(RequireContext())
                .SetTitle("Confirm deletion")
                .SetMessage($"Are you sure you want to delete '{transaction.Description}'?")
                .SetPositiveButton("Delete", async (s, e) =>
                {
                    await (_viewModel?.DeleteTransactionCommand.ExecuteAsync(transaction) ?? Task.CompletedTask);
                })
                .SetNegativeButton("Cancel", (s, e) => { })
                .Show();
        }

        private void OnCategorySelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (_viewModel == null || _suppressCategoryEvent)
                return;

            if (e.Position <= 0)
            {
                if (_viewModel.SelectedCategory != null)
                {
                    _viewModel.SelectedCategory = null;
                }

                return;
            }

            var categories = _viewModel.Categories;
            var index = e.Position - 1;
            if (index >= 0 && index < categories.Count)
            {
                var selected = categories[index];
                if (_viewModel.SelectedCategory == null || _viewModel.SelectedCategory.Id != selected.Id)
                {
                    _viewModel.SelectedCategory = selected;
                }
            }
        }

        private void OnRecurringCheckedChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (_viewModel == null || _suppressRecurringEvent)
                return;

            _viewModel.ShowOnlyRecurring = e.IsChecked;
        }

        private void OnDateAllClicked(object? sender, EventArgs e)
        {
            _viewModel?.SetDateFilterCommand.Execute(TransactionListViewModel.DateFilterOption.AllTime);
        }

        private void OnDateCurrentMonthClicked(object? sender, EventArgs e)
        {
            _viewModel?.SetDateFilterCommand.Execute(TransactionListViewModel.DateFilterOption.CurrentMonth);
        }

        private void OnDateLast30Clicked(object? sender, EventArgs e)
        {
            _viewModel?.SetDateFilterCommand.Execute(TransactionListViewModel.DateFilterOption.Last30Days);
        }

        /// <summary>
        /// Performs cleanup when the fragment is destroyed.
        /// </summary>
        public override void OnDestroyView()
        {
            // Detach adapter to avoid memory leaks
            if (_adapter != null)
            {
                _adapter.ItemClick -= OnTransactionClick;
                _adapter.EditClick -= OnTransactionEdit;
                _adapter.DeleteClick -= OnTransactionDelete;
            }

            if (_recyclerView != null && _scrollListener != null)
            {
                _recyclerView.RemoveOnScrollListener(_scrollListener);
            }

            if (_categorySpinner != null)
            {
                _categorySpinner.ItemSelected -= OnCategorySelected;
                _categorySpinner.Adapter = null;
            }

            if (_recurringCheckBox != null)
            {
                _recurringCheckBox.CheckedChange -= OnRecurringCheckedChanged;
            }

            if (_dateAllButton != null)
            {
                _dateAllButton.Click -= OnDateAllClicked;
            }

            if (_dateCurrentMonthButton != null)
            {
                _dateCurrentMonthButton.Click -= OnDateCurrentMonthClicked;
            }

            if (_dateLast30Button != null)
            {
                _dateLast30Button.Click -= OnDateLast30Clicked;
            }

            _categoryAdapter = null;

            _subscriptions?.Dispose();
            _subscriptions = null;
            _scrollListener = null;
            _layoutManager = null;

            base.OnDestroyView();
        }

        private sealed class EndlessScrollListener : RecyclerView.OnScrollListener
        {
            private readonly LinearLayoutManager _layoutManager;
            private readonly Func<bool> _hasMore;
            private readonly Action _loadMore;
            private bool _isLoading;

            public EndlessScrollListener(LinearLayoutManager layoutManager, Func<bool> hasMore, Action loadMore)
            {
                _layoutManager = layoutManager;
                _hasMore = hasMore;
                _loadMore = loadMore;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);

                if (dy <= 0 || _isLoading || !_hasMore())
                    return;

                var totalItemCount = _layoutManager.ItemCount;
                if (totalItemCount == 0)
                    return;

                var lastVisibleItem = _layoutManager.FindLastVisibleItemPosition();
                if (lastVisibleItem >= totalItemCount - 4)
                {
                    _isLoading = true;
                    _loadMore();
                }
            }

            public void SetLoading(bool isLoading) => _isLoading = isLoading;

            public void ResetState() => _isLoading = false;
        }
    }
}
