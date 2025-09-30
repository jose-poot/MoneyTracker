using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Base.Enums;
using MoneyTracker.Presentation.Base.Interfaces;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base;

internal class EditTextPropertyBinding<TViewModel, TProperty> : IBinding, IBindingModeAware
        where TViewModel : ObservableObject
{
    private readonly EditText _editText;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, TProperty>> _propertyExpression;
    private readonly Func<object, TProperty> _getter;
    private readonly Action<object, TProperty>? _setter;
    private readonly string _propertyName;

    public BindingMode Mode { get; set; } = BindingMode.OneWay;

    public EditTextPropertyBinding(EditText editText, TViewModel viewModel,
        Expression<Func<object, TProperty>> propertyExpression)
    {
        _editText = editText;
        _viewModel = viewModel;
        _propertyExpression = propertyExpression;
        _getter = propertyExpression.Compile();
        _setter = CreateSetter(propertyExpression);
        _propertyName = GetPropertyName(propertyExpression);
    }

    public void Apply()
    {
        UpdateView();
        _viewModel.PropertyChanged += OnPropertyChanged;

        if (Mode == BindingMode.TwoWay && _setter != null)
        {
            _editText.TextChanged += OnTextChanged;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName || e.PropertyName == null)
        {
            UpdateView();
        }
    }

    private void OnTextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (_setter != null && Mode == BindingMode.TwoWay)
        {
            try
            {
                var text = e.Text?.ToString() ?? string.Empty;
                var value = ConvertFromString<TProperty>(text);
                _setter(_viewModel, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Binding conversion error: {ex.Message}");
            }
        }
    }

    private void UpdateView()
    {
        var value = _getter(_viewModel);
        var newText = value?.ToString() ?? string.Empty;

        if (string.Equals(_editText.Text, newText, StringComparison.Ordinal))
            return;

        var selectionStart = _editText.SelectionStart;
        var selectionEnd = _editText.SelectionEnd;

        _editText.Text = newText;

        if (selectionStart >= 0 && selectionEnd >= 0)
        {
            var length = newText.Length;
            var clampedStart = Math.Clamp(selectionStart, 0, length);
            var clampedEnd = Math.Clamp(selectionEnd, 0, length);
            _editText.SetSelection(clampedStart, clampedEnd);
        }
    }

    private T ConvertFromString<T>(string input)
    {
        if (string.IsNullOrEmpty(input) && default(T) != null)
            return default(T)!;

        var type = typeof(T);
        var nullableType = Nullable.GetUnderlyingType(type);
        var targetType = nullableType ?? type;

        if (targetType == typeof(string))
            return (T)(object)input;

        return (T)Convert.ChangeType(input, targetType);
    }

    private string GetPropertyName<T>(Expression<Func<object, T>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    // Fix for the generic cast
    private Action<object, TProperty>? CreateSetter(Expression<Func<object, TProperty>> expression)
    {
        if (expression.Body is not MemberExpression member)
            return null;

        var param = Expression.Parameter(typeof(object));
        var valueParm = Expression.Parameter(typeof(TProperty));
        var convertedParam = Expression.Convert(param, member.Expression!.Type);
        var assign = Expression.Assign(Expression.Property(convertedParam, member.Member.Name), valueParm);

        return Expression.Lambda<Action<object, TProperty>>(assign, param, valueParm).Compile();
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= OnPropertyChanged;
        _editText.TextChanged -= OnTextChanged;
    }
}
