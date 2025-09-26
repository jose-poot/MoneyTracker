using Android.Views;
using MoneyTracker.Presentation.Base.Enums;

namespace MoneyTracker.Presentation.Base.Interfaces;

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

    public void Apply()
    {
        _binding.Apply();
    }
}