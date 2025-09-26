using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Base.Enums;
using MoneyTracker.Presentation.Base.Interfaces;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base;

internal class CheckBoxPropertyBinding<TViewModel> : IBinding, IBindingModeAware
        where TViewModel : ObservableObject
{
    private readonly CheckBox _checkBox;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, bool>>? _propertyExpression;
    private readonly Func<object, bool>? _getter;
    private readonly Action<object, bool>? _setter;
    private readonly string? _propertyName;

    public BindingMode Mode { get; set; } = BindingMode.OneWay;

    public CheckBoxPropertyBinding(CheckBox checkBox, TViewModel viewModel,
        Expression<Func<object, bool>>? propertyExpression)
    {
        _checkBox = checkBox;
        _viewModel = viewModel;
        _propertyExpression = propertyExpression;

        if (propertyExpression != null)
        {
            _getter = propertyExpression.Compile();
            _setter = CreateSetter(propertyExpression);
            _propertyName = GetPropertyName(propertyExpression);
        }
    }

    public void Apply()
    {
        if (_getter != null)
        {
            UpdateView();
            _viewModel.PropertyChanged += OnPropertyChanged;
        }

        if (Mode == BindingMode.TwoWay && _setter != null)
        {
            _checkBox.CheckedChange += OnCheckedChanged;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName || e.PropertyName == null)
        {
            UpdateView();
        }
    }

    private void OnCheckedChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
    {
        if (_setter != null && Mode == BindingMode.TwoWay)
        {
            _setter(_viewModel, e.IsChecked);
        }
    }

    private void UpdateView()
    {
        if (_getter != null)
        {
            _checkBox.Checked = _getter(_viewModel);
        }
    }

    private string GetPropertyName(Expression<Func<object, bool>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    private Action<object, bool>? CreateSetter(Expression<Func<object, bool>> expression)
    {
        if (expression.Body is not MemberExpression member)
            return null;

        var param = Expression.Parameter(typeof(object));
        var valueParm = Expression.Parameter(typeof(bool));
        var convertedParam = Expression.Convert(param, member.Expression!.Type);
        var assign = Expression.Assign(Expression.Property(convertedParam, member.Member.Name), valueParm);

        return Expression.Lambda<Action<object, bool>>(assign, param, valueParm).Compile();
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= OnPropertyChanged;
        _checkBox.CheckedChange -= OnCheckedChanged;
    }
}
