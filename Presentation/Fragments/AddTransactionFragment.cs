using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.Extensions;
using MoneyTracker.Presentation.ViewModels;
using Google.Android.Material.TextField;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using AndroidX.AppCompat.App;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace MoneyTracker.Presentation.Fragments
{
    /// <summary>
    /// Fragment para agregar o editar transacciones
    /// </summary>
    public class AddTransactionFragment : Fragment
    {
        private AddTransactionViewModel? _viewModel;
        private CompositeDisposable? _bindings;
        private CompositeDisposable? _viewModelSubscriptions;
        private CompositeDisposable? _uiEventSubscriptions;

        // Controles del formulario
        private TextInputLayout? _descriptionInputLayout;
        private EditText? _descriptionEditText;
        private TextInputLayout? _amountInputLayout;
        private EditText? _amountEditText;
        private LinearLayout? _typeChipGroup;
        private RadioButton? _incomeChip;
        private RadioButton? _expenseChip;

        private Spinner? _categorySpinner;
        private EditText? _dateEditText;
        private EditText? _notesEditText;
        private EditText? _locationEditText;
        private CheckBox? _recurringCheckBox;

        // ✅ CAMBIAR MaterialButton por Button genérico
        private Button? _saveButton;
        private Button? _cancelButton;
        private LinearLayout? _validationErrorsLayout;
        private EventHandler<AdapterView.ItemSelectedEventArgs>? _categorySelectedHandler;

        /// <summary>
        /// Crea una nueva instancia para editar una transacción
        /// </summary>
        public static AddTransactionFragment NewInstanceForEdit(TransactionDto transaction)
        {
            var fragment = new AddTransactionFragment();
            var args = new Bundle();
            args.PutString("transaction_json", Newtonsoft.Json.JsonConvert.SerializeObject(transaction));
            fragment.Arguments = args;
            return fragment;
        }

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_add_transaction, container, false);

            InitializeViews(view);
            SetupEventHandlers();

            return view;
        }

        public override void OnViewCreated(View view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Obtener ViewModel
            _viewModel = MoneyTrackerApplication.GetService<AddTransactionViewModel>();

            // Verificar si es modo edición
            CheckEditMode();

            BindViewModel();
        }

        /// <summary>
        /// Inicializa todas las vistas
        /// </summary>
        private void InitializeViews(View view)
        {
            _descriptionInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.input_layout_description);
            _descriptionEditText = view.FindViewById<EditText>(Resource.Id.edit_description);
            _amountInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.input_layout_amount);
            _amountEditText = view.FindViewById<EditText>(Resource.Id.edit_amount);
            _typeChipGroup = view.FindViewById<LinearLayout>(Resource.Id.chip_group_type);
            _incomeChip = view.FindViewById<RadioButton>(Resource.Id.chip_income_type);
            _expenseChip = view.FindViewById<RadioButton>(Resource.Id.chip_expense_type);

            _categorySpinner = view.FindViewById<Spinner>(Resource.Id.spinner_category);
            _dateEditText = view.FindViewById<EditText>(Resource.Id.edit_date);
            _notesEditText = view.FindViewById<EditText>(Resource.Id.edit_notes);
            _locationEditText = view.FindViewById<EditText>(Resource.Id.edit_location);
            _recurringCheckBox = view.FindViewById<CheckBox>(Resource.Id.checkbox_recurring);

            // ✅ CAMBIAR MaterialButton por Button
            _saveButton = view.FindViewById<Button>(Resource.Id.button_save);
            _cancelButton = view.FindViewById<Button>(Resource.Id.button_cancel);
            _validationErrorsLayout = view.FindViewById<LinearLayout>(Resource.Id.layout_validation_errors);
        }

        /// <summary>
        /// Configura los event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            _uiEventSubscriptions?.Dispose();
            _uiEventSubscriptions = new CompositeDisposable();

            if (_saveButton != null)
            {
                EventHandler handler = (s, e) => _viewModel?.SaveTransactionCommand.Execute(null);
                _saveButton.Click += handler;
                _uiEventSubscriptions.Add(new ActionDisposable(() => _saveButton.Click -= handler));
            }

            if (_cancelButton != null)
            {
                EventHandler handler = (s, e) => _viewModel?.CancelCommand.Execute(null);
                _cancelButton.Click += handler;
                _uiEventSubscriptions.Add(new ActionDisposable(() => _cancelButton.Click -= handler));
            }

            if (_incomeChip != null)
            {
                EventHandler<CompoundButton.CheckedChangeEventArgs> handler = (s, e) =>
                {
                    if (e.IsChecked && _viewModel != null)
                    {
                        _viewModel.TransactionType = TransactionType.Income;
                        if (_expenseChip != null)
                            _expenseChip.Checked = false;
                    }
                };

                _incomeChip.CheckedChange += handler;
                _uiEventSubscriptions.Add(new ActionDisposable(() => _incomeChip.CheckedChange -= handler));
            }

            if (_expenseChip != null)
            {
                EventHandler<CompoundButton.CheckedChangeEventArgs> handler = (s, e) =>
                {
                    if (e.IsChecked && _viewModel != null)
                    {
                        _viewModel.TransactionType = TransactionType.Expense;
                        if (_incomeChip != null)
                            _incomeChip.Checked = false;
                    }
                };

                _expenseChip.CheckedChange += handler;
                _uiEventSubscriptions.Add(new ActionDisposable(() => _expenseChip.CheckedChange -= handler));
            }

            if (_dateEditText != null)
            {
                EventHandler handler = ShowDatePicker;
                EventHandler<View.FocusChangeEventArgs> focusHandler = (s, e) =>
                {
                    if (e.HasFocus) ShowDatePicker(s, EventArgs.Empty);
                };

                _dateEditText.Click += handler;
                _dateEditText.FocusChange += focusHandler;

                _uiEventSubscriptions.Add(new ActionDisposable(() =>
                {
                    _dateEditText.Click -= handler;
                    _dateEditText.FocusChange -= focusHandler;
                }));
            }

            if (_recurringCheckBox != null)
            {
                EventHandler<CompoundButton.CheckedChangeEventArgs> handler = (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.IsRecurring = e.IsChecked;
                };

                _recurringCheckBox.CheckedChange += handler;
                _uiEventSubscriptions.Add(new ActionDisposable(() => _recurringCheckBox.CheckedChange -= handler));
            }
        }

        private void SetupBindings()
        {
            if (_viewModel == null) return;

            _bindings?.Dispose();
            _bindings = new CompositeDisposable();

            var culture = CultureInfo.CurrentCulture;

            if (_descriptionEditText != null)
            {
                _bindings.Add(_viewModel.BindTwoWayThrottled(
                    _descriptionEditText,
                    vm => vm.Description,
                    debounceMs: 150));
            }

            if (_amountEditText != null)
            {
                _bindings.Add(_viewModel.BindTwoWayThrottled(
                    _amountEditText,
                    vm => vm.Amount,
                    debounceMs: 150,
                    toStringConverter: value => value > 0 ? value.ToString("0.##", culture) : string.Empty,
                    fromStringConverter: text =>
                    {
                        if (decimal.TryParse(text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, culture, out var amount))
                        {
                            return amount;
                        }

                        return _viewModel.Amount;
                    }));
            }

            if (_notesEditText != null)
            {
                _bindings.Add(_viewModel.BindTwoWayThrottled(
                    _notesEditText,
                    vm => vm.Notes,
                    debounceMs: 200));
            }

            if (_locationEditText != null)
            {
                _bindings.Add(_viewModel.BindTwoWayThrottled(
                    _locationEditText,
                    vm => vm.Location,
                    debounceMs: 200));
            }
        }

        /// <summary>
        /// Verifica si está en modo edición y configura el ViewModel
        /// </summary>
        private void CheckEditMode()
        {
            if (Arguments?.GetString("transaction_json") is string json && !string.IsNullOrEmpty(json))
            {
                try
                {
                    var transaction = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionDto>(json);
                    if (transaction != null)
                    {
                        _viewModel?.SetEditMode(transaction);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing transaction JSON: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Conecta el ViewModel con la UI
        /// </summary>
        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModelSubscriptions?.Dispose();
            _viewModelSubscriptions = new CompositeDisposable();

            PropertyChangedEventHandler propertyChangedHandler = (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(AddTransactionViewModel.Title):
                            UpdateTitle();
                            break;
                        case nameof(AddTransactionViewModel.CanSave):
                            if (_saveButton != null)
                                _saveButton.Enabled = _viewModel.CanSave;
                            break;
                        case nameof(AddTransactionViewModel.IsBusy):
                            UpdateBusyState();
                            break;
                        case nameof(AddTransactionViewModel.ValidationErrors):
                        case nameof(AddTransactionViewModel.HasValidationErrors):
                            UpdateValidationErrors();
                            break;
                        case nameof(AddTransactionViewModel.TransactionType):
                            UpdateTransactionTypeSelection();
                            break;
                        case nameof(AddTransactionViewModel.TransactionDate):
                            if (_dateEditText != null)
                                _dateEditText.Text = _viewModel.TransactionDate.ToString("dd/MM/yyyy");
                            break;
                        case nameof(AddTransactionViewModel.IsRecurring):
                            if (_recurringCheckBox != null)
                                _recurringCheckBox.Checked = _viewModel.IsRecurring;
                            break;
                        case nameof(AddTransactionViewModel.SelectedCategory):
                            UpdateSelectedCategory();
                            break;
                    }
                });
            };

            _viewModel.PropertyChanged += propertyChangedHandler;
            _viewModelSubscriptions.Add(new ActionDisposable(() => _viewModel.PropertyChanged -= propertyChangedHandler));

            NotifyCollectionChangedEventHandler categoriesChangedHandler = (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    SetupCategorySpinner();
                    UpdateSelectedCategory();
                });
            };

            _viewModel.Categories.CollectionChanged += categoriesChangedHandler;
            _viewModelSubscriptions.Add(new ActionDisposable(() => _viewModel.Categories.CollectionChanged -= categoriesChangedHandler));

            NotifyCollectionChangedEventHandler validationChangedHandler = (s, e) =>
            {
                Activity?.RunOnUiThread(UpdateValidationErrors);
            };

            _viewModel.ValidationErrors.CollectionChanged += validationChangedHandler;
            _viewModelSubscriptions.Add(new ActionDisposable(() => _viewModel.ValidationErrors.CollectionChanged -= validationChangedHandler));

            LoadInitialValues();
            SetupCategorySpinner();
            UpdateSelectedCategory();
            SetupBindings();
            UpdateBusyState();
            UpdateValidationErrors();
        }

        /// <summary>
        /// Carga los valores iniciales del ViewModel a la UI
        /// </summary>
        private void LoadInitialValues()
        {
            if (_viewModel == null) return;

            // Cargar valores del ViewModel
            if (_dateEditText != null)
                _dateEditText.Text = _viewModel.TransactionDate.ToString("dd/MM/yyyy");

            if (_recurringCheckBox != null)
                _recurringCheckBox.Checked = _viewModel.IsRecurring;

            UpdateTransactionTypeSelection();
            UpdateTitle();

            if (_saveButton != null)
                _saveButton.Enabled = _viewModel.CanSave;
        }

        /// <summary>
        /// Configura el spinner de categorías
        /// </summary>
        private void SetupCategorySpinner()
        {
            if (_categorySpinner == null || _viewModel == null) return;

            var categories = _viewModel.Categories.ToList();
            var adapter = new ArrayAdapter<CategoryDto>(
                RequireContext(),
                Android.Resource.Layout.SimpleSpinnerItem,
                categories
            );

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _categorySpinner.Adapter = adapter;

            // Seleccionar categoría actual
            UpdateSelectedCategory();

            if (_categorySelectedHandler != null)
            {
                _categorySpinner.ItemSelected -= _categorySelectedHandler;
            }

            _categorySelectedHandler = (s, e) =>
            {
                if (e.Position >= 0 && e.Position < categories.Count)
                {
                    _viewModel.SelectedCategory = categories[e.Position];
                }
            };

            _categorySpinner.ItemSelected += _categorySelectedHandler;
        }

        private void UpdateTitle()
        {
            if (_viewModel == null) return;

            if (Activity is AppCompatActivity activity && activity.SupportActionBar != null)
            {
                activity.SupportActionBar.Title = _viewModel.Title;
            }
        }

        private void UpdateTransactionTypeSelection()
        {
            if (_viewModel == null) return;

            var isIncome = _viewModel.TransactionType == TransactionType.Income;

            if (_incomeChip != null && _incomeChip.Checked != isIncome)
            {
                _incomeChip.Checked = isIncome;
            }

            if (_expenseChip != null && _expenseChip.Checked == isIncome)
            {
                _expenseChip.Checked = !isIncome;
            }
        }

        private void UpdateSelectedCategory()
        {
            if (_viewModel == null || _categorySpinner == null) return;

            var categories = _viewModel.Categories.ToList();
            if (categories.Count == 0)
                return;

            var selectedId = _viewModel.SelectedCategory?.Id;
            if (selectedId == null)
                return;

            var index = categories.FindIndex(c => c.Id == selectedId);
            if (index >= 0 && _categorySpinner.SelectedItemPosition != index)
            {
                _categorySpinner.SetSelection(index);
            }
        }

        /// <summary>
        /// Actualiza el estado de loading
        /// </summary>
        private void UpdateBusyState()
        {
            if (_viewModel == null) return;

            var isBusy = _viewModel.IsBusy;

            // Deshabilitar controles durante operaciones
            if (_saveButton != null)
                _saveButton.Enabled = !isBusy && _viewModel.CanSave;

            if (_cancelButton != null)
                _cancelButton.Enabled = !isBusy;
        }

        /// <summary>
        /// Actualiza la visualización de errores de validación
        /// </summary>
        private void UpdateValidationErrors()
        {
            if (_validationErrorsLayout == null || _viewModel == null) return;

            // Limpiar errores anteriores
            _validationErrorsLayout.RemoveAllViews();

            if (!_viewModel.HasValidationErrors)
            {
                _validationErrorsLayout.Visibility = ViewStates.Gone;
                return;
            }

            _validationErrorsLayout.Visibility = ViewStates.Visible;

            // Agregar cada error como TextView
            foreach (var error in _viewModel.ValidationErrors)
            {
                var errorView = new TextView(RequireContext())
                {
                    Text = $"• {error}",
                    TextSize = 14
                };
                errorView.SetTextColor(Android.Graphics.Color.Red);
                _validationErrorsLayout.AddView(errorView);
            }
        }

        /// <summary>
        /// Muestra el selector de fecha
        /// </summary>
        private void ShowDatePicker(object? sender, EventArgs e)
        {
            if (_viewModel == null) return;

            var date = _viewModel.TransactionDate;

            var datePickerDialog = new Android.App.DatePickerDialog(
                RequireContext(),
                (s, e) =>
                {
                    _viewModel.TransactionDate = new DateTime(e.Year, e.Month + 1, e.DayOfMonth);
                    if (_dateEditText != null)
                        _dateEditText.Text = _viewModel.TransactionDate.ToString("dd/MM/yyyy");
                },
                date.Year,
                date.Month - 1,
                date.Day
            );

            datePickerDialog.Show();
        }

        public override void OnDestroyView()
        {
            if (_categorySpinner != null && _categorySelectedHandler != null)
            {
                _categorySpinner.ItemSelected -= _categorySelectedHandler;
                _categorySelectedHandler = null;
            }

            _bindings?.Dispose();
            _bindings = null;

            _viewModelSubscriptions?.Dispose();
            _viewModelSubscriptions = null;

            _uiEventSubscriptions?.Dispose();
            _uiEventSubscriptions = null;

            base.OnDestroyView();
        }
    }
}
