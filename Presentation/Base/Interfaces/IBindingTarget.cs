using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MoneyTracker.Presentation.Base.Interfaces;

public interface IBindingTarget<TView> where TView : View
{
    IBindingTarget<TView> TwoWay();
    IBindingTarget<TView> Source();
    IBindingTarget<TView> WithItemsSource<TItem>(
        Expression<Func<object, IEnumerable<TItem>>> itemsExpression,
        Func<Context, IList<TItem>, SpinnerAdapter>? adapterFactory = null);
    void Apply();
}
