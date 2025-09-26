using Android.OS;
using CommunityToolkit.Mvvm.ComponentModel;
using MoneyTracker.Presentation.Extensions;
using System.ComponentModel;
using System.Linq.Expressions;
using MoneyTracker.Presentation.ViewModels;


public static partial class ThreadSafeBindingExtensions
{
    private static readonly Handler MainHandler = new(Looper.MainLooper!);

    public static IDisposable BindTwoWayThrottled<TViewModel, TProperty>(
        this TViewModel viewModel,
        TextView view,
        Expression<Func<TViewModel, TProperty>> propertyExpression,
        int debounceMs = 300,
        Func<TProperty, string>? toStringConverter = null,
        Func<string, TProperty>? fromStringConverter = null)
        where TViewModel : ObservableObject
    {
        var propertyName = GetPropertyName(propertyExpression);
        var getter = propertyExpression.Compile();
        var setter = CreateSetter(propertyExpression);

        toStringConverter ??= value => value?.ToString() ?? string.Empty;
        fromStringConverter ??= CreateDefaultFromStringConverter<TProperty>();

        // ✅ CORRECCIÓN: Clase nullable sin constraint
        var currentRunnable = new AtomicReference<Java.Lang.Runnable>();

        void UpdateView()
        {
            if (Looper.MyLooper() == Looper.MainLooper)
            {
                var value = getter(viewModel);
                view.Text = toStringConverter(value);
            }
            else
            {
                MainHandler.Post(() =>
                {
                    var value = getter(viewModel);
                    view.Text = toStringConverter(value);
                });
            }
        }

        UpdateView();

        PropertyChangedEventHandler propertyChangedHandler = (s, e) =>
        {
            if (e.PropertyName == propertyName || e.PropertyName == null)
                UpdateView();
        };
        viewModel.PropertyChanged += propertyChangedHandler;

        EventHandler<Android.Text.TextChangedEventArgs>? textChangedHandler = null;
        if (view is EditText editText && setter != null)
        {
            textChangedHandler = (s, e) =>
            {
                // ✅ CORRECCIÓN: Variable 'text' definida correctamente
                var text = e.Text?.ToString() ?? string.Empty;

                var previousRunnable = currentRunnable.GetAndSet(null);
                if (previousRunnable != null)
                    MainHandler.RemoveCallbacks(previousRunnable);

                var newRunnable = new Java.Lang.Runnable(() =>
                {
                    try
                    {
                        var newValue = fromStringConverter(text);
                        setter(viewModel, newValue);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Binding conversion error: {ex.Message}");
                    }
                });

                currentRunnable.Set(newRunnable);
                MainHandler.PostDelayed(newRunnable, debounceMs);
            };
            editText.TextChanged += textChangedHandler;
        }

        return new CompositeDisposable(
            new ActionDisposable(() => viewModel.PropertyChanged -= propertyChangedHandler),
            new ActionDisposable(() =>
            {
                if (textChangedHandler != null && view is EditText et)
                    et.TextChanged -= textChangedHandler;

                var runnable = currentRunnable.GetAndSet(null);
                if (runnable != null)
                    MainHandler.RemoveCallbacks(runnable);
            })
        );
    }

    // ✅ CORRECCIÓN: AtomicReference sin nullable constraint


    private static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        return expression.Body is MemberExpression member
            ? member.Member.Name
            : throw new ArgumentException("Expression must be a property access");
    }

    private static Action<T, TProperty>? CreateSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is not MemberExpression member)
            return null;

        var param = Expression.Parameter(typeof(T));
        var valueParm = Expression.Parameter(typeof(TProperty));
        var assign = Expression.Assign(Expression.Property(param, member.Member.Name), valueParm);

        return Expression.Lambda<Action<T, TProperty>>(assign, param, valueParm).Compile();
    }

    private static Func<string, TProperty> CreateDefaultFromStringConverter<TProperty>()
    {
        var type = typeof(TProperty);
        var nullableType = Nullable.GetUnderlyingType(type);

        return input =>
        {
            if (string.IsNullOrEmpty(input) && (nullableType != null || !type.IsValueType))
                return default!;

            var targetType = nullableType ?? type;

            if (targetType == typeof(string))
                return (TProperty)(object)input;

            try
            {
                return (TProperty)Convert.ChangeType(input, targetType);
            }
            catch
            {
                return default!;
            }
        };
    }
}