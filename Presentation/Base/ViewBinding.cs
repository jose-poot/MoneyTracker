using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.Base.Interfaces;
using System.Linq.Expressions;
using Android.Views;

internal class ViewBinding<TView, TViewModel> : IViewBinding<TView>, IBinding
        where TView : View
        where TViewModel : ObservableObject
{
    private readonly TView _view;
    private readonly TViewModel _viewModel;
    private readonly DataBindingSet<TViewModel> _bindingSet;
    private IBinding? _actualBinding;

    public ViewBinding(TView view, TViewModel viewModel, DataBindingSet<TViewModel> bindingSet)
    {
        _view = view;
        _viewModel = viewModel;
        _bindingSet = bindingSet;
    }

    public IBindingTarget<TView> To<TProperty>(Expression<Func<object, TProperty>> vmProperty)
    {
        _actualBinding = CreatePropertyBinding(vmProperty);
        _bindingSet.AddBinding(_actualBinding);
        return new MoneyTracker.Presentation.Base.BindingTarget<TView>(_view, _actualBinding);
    }

    public IBindingTarget<TView> To(Expression<Func<object, IRelayCommand>> command)
    {
        _actualBinding = CreateCommandBinding(command);
        _bindingSet.AddBinding(_actualBinding);
        return new MoneyTracker.Presentation.Base.BindingTarget<TView>(_view, _actualBinding);
    }

    private IBinding CreatePropertyBinding<TProperty>(Expression<Func<object, TProperty>> vmProperty)
    {
        return _view switch
        {
            TextView textView when _view is not EditText => new TextViewPropertyBinding<TViewModel, TProperty>(
                textView, _viewModel, vmProperty),
            EditText editText => new EditTextPropertyBinding<TViewModel, TProperty>(
                editText, _viewModel, vmProperty),
            CheckBox checkBox when typeof(TProperty) == typeof(bool) =>
                new CheckBoxPropertyBinding<TViewModel>(checkBox, _viewModel,
                    vmProperty as Expression<Func<object, bool>>),
            _ => throw new NotSupportedException($"Binding not supported for {typeof(TView).Name}")
        };
    }

    private IBinding CreateCommandBinding(Expression<Func<object, IRelayCommand>> command)
    {
        return new CommandBinding<TView, TViewModel>(_view, _viewModel, command);
    }

    public void Apply()
    {
        _actualBinding?.Apply();
    }

    public void Dispose()
    {
        _actualBinding?.Dispose();
    }
}