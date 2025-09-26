using System.Linq.Expressions;
using Android.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Presentation.Base.Interfaces;

namespace MoneyTracker.Presentation.Base;

internal class ViewPropertyBinding<TView, TViewModel> : IViewBinding<TView>, IBinding
    where TView : View
    where TViewModel : ObservableObject
{
    private readonly TView _view;
    private readonly Expression<Func<TView, object>> _viewProperty;
    private readonly TViewModel _viewModel;
    private readonly DataBindingSet<TViewModel> _bindingSet;

    public ViewPropertyBinding(TView view, Expression<Func<TView, object>> viewProperty,
        TViewModel viewModel, DataBindingSet<TViewModel> bindingSet)
    {
        _view = view;
        _viewProperty = viewProperty;
        _viewModel = viewModel;
        _bindingSet = bindingSet;
    }

    public IBindingTarget<TView> To<TProperty>(Expression<Func<object, TProperty>> vmProperty)
    {
        // Para propiedades específicas de la vista (como Enabled, Visibility, etc.)
        var binding = new ViewSpecificPropertyBinding<TView, TViewModel, TProperty>(
            _view, _viewProperty, _viewModel, vmProperty);
        _bindingSet.AddBinding(binding);
        return new BindingTarget<TView>(_view, binding);
    }

    public IBindingTarget<TView> To(Expression<Func<object, IRelayCommand>> command)
    {
        throw new NotSupportedException("Cannot bind view properties to commands");
    }

    public void Apply() { }
    public void Dispose() { }
}