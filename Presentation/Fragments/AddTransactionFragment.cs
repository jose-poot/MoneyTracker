using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;
using MoneyTracker.Presentation.ViewModels;
using Google.Android.Material.TextField;
using System;
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
            // Botones principales
            if (_saveButton != null)
                _saveButton.Click += (s, e) => _viewModel?.SaveTransactionCommand.Execute(null);

            if (_cancelButton != null)
                _cancelButton.Click += (s, e) => _viewModel?.CancelCommand.Execute(null);

            // Campos de texto
            if (_descriptionEditText != null)
            {
                _descriptionEditText.TextChanged += (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.Description = e?.Text?.ToString() ?? string.Empty;
                };
            }

            if (_amountEditText != null)
            {
                _amountEditText.TextChanged += (s, e) =>
                {
                    if (_viewModel != null && decimal.TryParse(e?.Text?.ToString(), out var amount))
                        _viewModel.Amount = amount;
                };
            }

            // RadioButtons para tipo de transacción
            if (_incomeChip != null)
            {
                _incomeChip.CheckedChange += (s, e) =>
                {
                    if (e.IsChecked && _viewModel != null)
                    {
                        _viewModel.TransactionType = TransactionType.Income;
                        // Desmarcar el otro
                        if (_expenseChip != null)
                            _expenseChip.Checked = false;
                    }
                };
            }

            if (_expenseChip != null)
            {
                _expenseChip.CheckedChange += (s, e) =>
                {
                    if (e.IsChecked && _viewModel != null)
                    {
                        _viewModel.TransactionType = TransactionType.Expense;
                        // Desmarcar el otro
                        if (_incomeChip != null)
                            _incomeChip.Checked = false;
                    }
                };
            }

            // Date picker
            if (_dateEditText != null)
            {
                _dateEditText.Click += ShowDatePicker;
                _dateEditText.FocusChange += (s, e) =>
                {
                    if (e.HasFocus) ShowDatePicker(s, EventArgs.Empty);
                };
            }

            // Otros campos
            if (_notesEditText != null)
            {
                _notesEditText.TextChanged += (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.Notes = e?.Text?.ToString() ?? string.Empty;
                };
            }

            if (_locationEditText != null)
            {
                _locationEditText.TextChanged += (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.Location = e?.Text?.ToString() ?? string.Empty;
                };
            }

            if (_recurringCheckBox != null)
            {
                _recurringCheckBox.CheckedChange += (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.IsRecurring = e.IsChecked;
                };
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

            // Observar cambios en propiedades
            _viewModel.PropertyChanged += (s, e) =>
            {
                Activity?.RunOnUiThread(() =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(AddTransactionViewModel.Title):
                            if (Activity is AppCompatActivity activity && activity.SupportActionBar != null)
                            {
                                activity.SupportActionBar.Title = _viewModel.Title;
                            }
                            break;
                        case nameof(AddTransactionViewModel.CanSave):
                            if (_saveButton != null)
                                _saveButton.Enabled = _viewModel.CanSave;
                            break;
                        case nameof(AddTransactionViewModel.IsBusy):
                            UpdateBusyState();
                            break;
                        case nameof(AddTransactionViewModel.ValidationErrors):
                            UpdateValidationErrors();
                            break;
                    }
                });
            };

            // Observar cambios en categorías
            _viewModel.Categories.CollectionChanged += (s, e) =>
            {
                Activity?.RunOnUiThread(SetupCategorySpinner);
            };

            // Configuración inicial
            LoadInitialValues();
            SetupCategorySpinner();
        }

        /// <summary>
        /// Carga los valores iniciales del ViewModel a la UI
        /// </summary>
        private void LoadInitialValues()
        {
            if (_viewModel == null) return;

            // Cargar valores del ViewModel
            if (_descriptionEditText != null)
                _descriptionEditText.Text = _viewModel.Description;

            if (_amountEditText != null)
                _amountEditText.Text = _viewModel.Amount > 0 ? _viewModel.Amount.ToString() : string.Empty;

            if (_dateEditText != null)
                _dateEditText.Text = _viewModel.TransactionDate.ToString("dd/MM/yyyy");

            if (_notesEditText != null)
                _notesEditText.Text = _viewModel.Notes;

            if (_locationEditText != null)
                _locationEditText.Text = _viewModel.Location;

            if (_recurringCheckBox != null)
                _recurringCheckBox.Checked = _viewModel.IsRecurring;

            // Seleccionar chip de tipo
            if (_viewModel.TransactionType == TransactionType.Income)
            {
                if (_incomeChip != null)
                    _incomeChip.Checked = true;
                if (_expenseChip != null)
                    _expenseChip.Checked = false;
            }
            else
            {
                if (_expenseChip != null)
                    _expenseChip.Checked = true;
                if (_incomeChip != null)
                    _incomeChip.Checked = false;
            }

            // Título de la actividad
            if (Activity is AppCompatActivity activity && activity.SupportActionBar != null)
            {
                activity.SupportActionBar.Title = _viewModel.Title;
            }
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
            if (_viewModel.SelectedCategory != null)
            {
                var index = categories.FindIndex(c => c.Id == _viewModel.SelectedCategory.Id);
                if (index >= 0)
                {
                    _categorySpinner.SetSelection(index);
                }
            }

            // Event handler para selección
            _categorySpinner.ItemSelected += (s, e) =>
            {
                if (e.Position >= 0 && e.Position < categories.Count)
                {
                    _viewModel.SelectedCategory = categories[e.Position];
                }
            };
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
    }
}