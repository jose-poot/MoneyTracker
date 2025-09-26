using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq.Expressions;
using Android.Views;
using MoneyTracker.Presentation.Base.Interfaces;
using static Android.Renderscripts.ScriptGroup;

namespace MoneyTracker.Presentation.Base;

public class DataBindingSet<TViewModel> : IDisposable where TViewModel : ObservableObject
{
    private readonly TViewModel _viewModel;
    private readonly List<IBinding> _bindings = new();
    private bool _applied = false;

    public DataBindingSet(TViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public IViewBinding<TView> Bind<TView>(TView view) where TView : View
    {
        return new ViewBinding<TView, TViewModel>(view, _viewModel, this);
    }

    public IViewBinding<TView> Bind<TView>(TView view, Expression<Func<TView, object>> viewProperty)
        where TView : View
    {
        return new ViewPropertyBinding<TView, TViewModel>(view, viewProperty, _viewModel, this);
    }

    internal void AddBinding(IBinding binding)
    {
        _bindings.Add(binding);
    }

    public void Apply()
    {
        if (_applied) return;

        foreach (var binding in _bindings)
        {
            binding.Apply();
        }

        _applied = true;
    }

    public void Dispose()
    {
        foreach (var binding in _bindings)
        {
            binding?.Dispose();
        }

        _bindings.Clear();
    }
}