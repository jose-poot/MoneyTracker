using Android.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Base.Interfaces;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base;

internal class ViewSpecificPropertyBinding<TView, TViewModel, TProperty> : IBinding
        where TView : View
        where TViewModel : ObservableObject
{
    private readonly TView _view;
    private readonly Expression<Func<TView, object>> _viewProperty;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, TProperty>> _vmProperty;
    private readonly Func<object, TProperty> _getter;
    private readonly string _propertyName;
    private readonly Action<TView, TProperty> _viewSetter;

    public ViewSpecificPropertyBinding(TView view, Expression<Func<TView, object>> viewProperty,
        TViewModel viewModel, Expression<Func<object, TProperty>> vmProperty)
    {
        _view = view;
        _viewProperty = viewProperty;
        _viewModel = viewModel;
        _vmProperty = vmProperty;
        _getter = vmProperty.Compile();
        _propertyName = GetPropertyName(vmProperty);
        _viewSetter = CreateViewSetter();
    }

    public void Apply()
    {
        UpdateView();
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName || e.PropertyName == null)
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        var value = _getter(_viewModel);
        _viewSetter(_view, value);
    }

    private Action<TView, TProperty> CreateViewSetter()
    {
        var viewPropertyName = GetViewPropertyName(_viewProperty);

        return viewPropertyName switch
        {
            "Enabled" when typeof(TProperty) == typeof(bool) =>
                (view, value) => view.Enabled = (bool)(object)value!,
            "Visibility" when typeof(TProperty) == typeof(bool) =>
                (view, value) => view.Visibility = (bool)(object)value! ? ViewStates.Visible : ViewStates.Gone,
            _ => throw new NotSupportedException($"Binding to {viewPropertyName} not supported")
        };
    }

    private string GetViewPropertyName(Expression<Func<TView, object>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        if (expression.Body is UnaryExpression { Operand: MemberExpression memberExpr })
            return memberExpr.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    private string GetPropertyName<T>(Expression<Func<object, T>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= OnPropertyChanged;
    }
}