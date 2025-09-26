using CommunityToolkit.Mvvm.Input;
using System.Linq.Expressions;
using Android.Views;

namespace MoneyTracker.Presentation.Base.Interfaces;

public interface IViewBinding<TView> where TView : View
{
    IBindingTarget<TView> To<TProperty>(Expression<Func<object, TProperty>> vmProperty);
    IBindingTarget<TView> To(Expression<Func<object, IRelayCommand>> command);
}