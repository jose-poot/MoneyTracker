using System.ComponentModel;
using System.Linq.Expressions;
using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Base.Enums;
using MoneyTracker.Presentation.Base.Interfaces;

namespace MoneyTracker.Presentation.Base;

internal class TextViewPropertyBinding<TViewModel, TProperty> : IBinding, IBindingModeAware
    where TViewModel : ObservableObject
{
    private readonly TextView _textView;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, TProperty>> _propertyExpression;
    private readonly Func<object, TProperty> _getter;
    private readonly string _propertyName;

    public BindingMode Mode { get; set; } = BindingMode.OneWay;

    public TextViewPropertyBinding(TextView textView, TViewModel viewModel,
        Expression<Func<object, TProperty>> propertyExpression)
    {
        _textView = textView;
        _viewModel = viewModel;
        _propertyExpression = propertyExpression;
        _getter = propertyExpression.Compile();
        _propertyName = GetPropertyName(propertyExpression);
    }

    public void Apply()
    {
        // Initial value
        UpdateView();

        // Subscribe to property changes
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
        _textView.Text = value?.ToString() ?? string.Empty;
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