using Android.Content;
using AndroidX.AppCompat.App;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Presentation.Extensions;
using MoneyTracker.Presentation.Services.Interfaces;

namespace MoneyTracker.Presentation.Base;

public abstract class ActivityBase<TViewModel> : AppCompatActivity
        where TViewModel : ObservableObject
{
    private CompositeDisposable? _subscriptions;
    private DataBindingSet<TViewModel>? _bindingSet;

    protected TViewModel ViewModel { get; private set; } = default!;

    protected IDialogService DialogService { get; private set; } = default!;
    protected INavigationService NavigationService { get; private set; } = default!;
    protected ICacheService CacheService { get; private set; } = default!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Initialize services
        var serviceProvider = MoneyTrackerApplication.ServiceProvider!;
        ViewModel = serviceProvider.GetRequiredService<TViewModel>();
        DialogService = serviceProvider.GetRequiredService<IDialogService>();
        NavigationService = serviceProvider.GetRequiredService<INavigationService>();
        CacheService = serviceProvider.GetRequiredService<ICacheService>();

        _subscriptions = new CompositeDisposable();

        // Load parameters if any
        LoadParameters();
    }

    /// <summary>
    /// Crea el conjunto de data binding siguiendo el patrón del arquitecto
    /// </summary>
    protected DataBindingSet<TViewModel> CreateDataBindingSet()
    {
        _bindingSet = new DataBindingSet<TViewModel>(ViewModel);
        return _bindingSet;
    }

    /// <summary>
    /// Aplica los data bindings creados
    /// </summary>
    protected void ApplyDataBindings()
    {
        if (_bindingSet != null)
        {
            _subscriptions?.Add(_bindingSet);
        }
    }

    protected virtual void LoadParameters()
    {
        var parametersJson = Intent?.GetStringExtra("parameters");
        if (!string.IsNullOrEmpty(parametersJson))
        {
            try
            {
                var parameters = Newtonsoft.Json.JsonConvert.DeserializeObject(parametersJson);
                OnParametersLoaded(parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading parameters: {ex.Message}");
            }
        }
    }

    protected virtual void OnParametersLoaded(object? parameters) { }

    protected override void OnDestroy()
    {
        _subscriptions?.Dispose();
        _bindingSet?.Dispose();

        base.OnDestroy();
    }
}