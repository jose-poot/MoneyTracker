using System.Linq.Expressions;
using Android.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Presentation.Base.Interfaces;

namespace MoneyTracker.Presentation.Base;

internal class CommandBinding<TView, TViewModel> : IBinding
    where TView : View
    where TViewModel : ObservableObject
{
    private readonly TView _view;
    private readonly TViewModel _viewModel;
    private readonly Expression<Func<object, IRelayCommand>> _commandExpression;
    private readonly Func<object, IRelayCommand> _getter;

    public CommandBinding(TView view, TViewModel viewModel, Expression<Func<object, IRelayCommand>> commandExpression)
    {
        _view = view;
        _viewModel = viewModel;
        _commandExpression = commandExpression;
        _getter = commandExpression.Compile();
    }

    public void Apply()
    {
        var command = _getter(_viewModel);

        _view.Click += (s, e) =>
        {
            if (command.CanExecute(null))
                command.Execute(null);
        };

        command.CanExecuteChanged += (s, e) =>
        {
            _view.Enabled = command.CanExecute(null);
        };

        _view.Enabled = command.CanExecute(null);
    }

    public void Dispose()
    {
        // Event handlers are automatically cleaned up when the view is destroyed
    }
}