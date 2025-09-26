using Android.Views;

namespace MoneyTracker.Presentation.Base.Interfaces;

public interface IBindingTarget<TView> where TView : View
{
    IBindingTarget<TView> TwoWay();
    IBindingTarget<TView> Source();
    void Apply();
}