using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MoneyTracker.Presentation.Binding;

public sealed class BindingSet<TViewModel> where TViewModel : ObservableObject
{
    private readonly TViewModel _vm;
    private readonly System.Collections.Generic.List<IDisposable> _subscriptions = new();

    public BindingSet(TViewModel vm) => _vm = vm;

    public IDisposable BindText(TextView view, Func<TViewModel, string?> getter, Action<TViewModel, string?>? setter = null)
    {
        void push() => view.Text = getter(_vm) ?? string.Empty;
        push();

        System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName is null) { push(); return; }
            var expected = getter.Method.Name.Replace("get_", "");
            if (e.PropertyName == expected) push();
        };
        _vm.PropertyChanged += handler;
        var disp1 = new ActionDisposable(() => _vm.PropertyChanged -= handler);
        _subscriptions.Add(disp1);

        if (setter is not null && view is EditText et)
        {
            et.TextChanged += (s, e) => setter(_vm, et.Text?.ToString());
            var disp2 = new ActionDisposable(() => et.TextChanged -= (s, e) => setter(_vm, et.Text?.ToString()));
            _subscriptions.Add(disp2);
        }

        return new ActionDisposable(Clear);
    }

    public IDisposable BindClick(Button button, IRelayCommand command)
    {
        EventHandler? handler = null;
        handler = (s, e) =>
        {
            if (command.CanExecute(null)) command.Execute(null);
        };
        button.Click += handler;
        var disp = new ActionDisposable(() => button.Click -= handler!);
        _subscriptions.Add(disp);
        return disp;
    }

    public void Clear()
    {
        foreach (var s in _subscriptions) s.Dispose();
        _subscriptions.Clear();
    }

    private sealed class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        public ActionDisposable(Action action) => _action = action;
        public void Dispose() => _action();
    }
}