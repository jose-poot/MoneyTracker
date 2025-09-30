using Android.Content;
using Android.Views;
using Android.Widget;
using MoneyTracker.Presentation.Base.Enums;
using MoneyTracker.Presentation.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base;

internal class BindingTarget<TView> : IBindingTarget<TView> where TView : View
{
    private readonly TView _view;
    private readonly IBinding _binding;
    private BindingMode _mode = BindingMode.OneWay;

    public BindingTarget(TView view, IBinding binding)
    {
        _view = view;
        _binding = binding;
    }

    public IBindingTarget<TView> TwoWay()
    {
        _mode = BindingMode.TwoWay;
        if (_binding is IBindingModeAware modeAware)
            modeAware.Mode = _mode;
        return this;
    }

    public IBindingTarget<TView> Source()
    {
        _mode = BindingMode.Source;
        if (_binding is IBindingModeAware modeAware)
            modeAware.Mode = _mode;
        return this;
    }

    public IBindingTarget<TView> WithItemsSource<TItem>(
        Expression<Func<object, IEnumerable<TItem>>> itemsExpression,
        Func<Context, IList<TItem>, SpinnerAdapter>? adapterFactory = null)
    {
        if (_view is not Spinner)
        {
            throw new InvalidOperationException("ItemsSource binding is only supported for Spinner views.");
        }

        if (_binding is not ISpinnerBinding<TItem> spinnerBinding)
        {
            throw new InvalidOperationException("Binding does not support Spinner items source configuration.");
        }

        spinnerBinding.SetItemsSource(itemsExpression);

        if (adapterFactory != null)
        {
            spinnerBinding.SetAdapterFactory(adapterFactory);
        }

        return this;
    }

    public void Apply()
    {
        _binding.Apply();
    }
}
