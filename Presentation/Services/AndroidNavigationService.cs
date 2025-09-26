using Android.Content;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Presentation.Activities;
using MoneyTracker.Presentation.Fragments;
using MoneyTracker.Presentation.Services.Interfaces;
using MoneyTracker.Presentation.ViewModels;

namespace MoneyTracker.Presentation.Services;

public class AndroidNavigationService : INavigationService
{
    private readonly Func<Activity> _currentActivityProvider;
    private readonly IServiceProvider _serviceProvider;

    public AndroidNavigationService(Func<Activity> currentActivityProvider, IServiceProvider serviceProvider)
    {
        _currentActivityProvider = currentActivityProvider;
        _serviceProvider = serviceProvider;
    }

    public bool CanNavigateBack
    {
        get
        {
            var activity = _currentActivityProvider();
            return activity is AndroidX.AppCompat.App.AppCompatActivity appActivity &&
                   appActivity.SupportFragmentManager.BackStackEntryCount > 0;
        }
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        return NavigateToAsync<TViewModel>(null);
    }

    public Task NavigateToAsync<TViewModel>(object parameters) where TViewModel : BaseViewModel
    {
        var viewModelType = typeof(TViewModel);
        var route = GetRouteFromViewModelType(viewModelType);
        return NavigateToAsync(route, parameters);
    }

    public Task NavigateToAsync(string route, object? parameters = null)
    {
        var activity = _currentActivityProvider();

        return route switch
        {
            "AddTransaction" => NavigateToFragment<AddTransactionFragment>(parameters),
            "EditTransaction" => NavigateToFragment<AddTransactionFragment>(parameters),
            "Settings" => NavigateToActivity<SettingsActivity>(parameters),
            _ => throw new ArgumentException($"Unknown route: {route}")
        };
    }

    public Task NavigateBackAsync()
    {
        var activity = _currentActivityProvider();

        if (activity is AndroidX.AppCompat.App.AppCompatActivity appActivity)
        {
            if (appActivity.SupportFragmentManager.BackStackEntryCount > 0)
            {
                appActivity.SupportFragmentManager.PopBackStack();
            }
            else
            {
                activity.Finish();
            }
        }

        return Task.CompletedTask;
    }

    public Task NavigateToRootAsync()
    {
        var activity = _currentActivityProvider();

        if (activity is AndroidX.AppCompat.App.AppCompatActivity appActivity)
        {
            // Clear back stack
            appActivity.SupportFragmentManager.PopBackStack(null,
                AndroidX.Fragment.App.FragmentManager.PopBackStackInclusive);
        }

        return Task.CompletedTask;
    }

    private Task NavigateToFragment<TFragment>(object? parameters) where TFragment : AndroidX.Fragment.App.Fragment, new()
    {
        var activity = _currentActivityProvider();

        if (activity is AndroidX.AppCompat.App.AppCompatActivity appActivity)
        {
            var fragment = new TFragment();

            if (parameters != null)
            {
                var args = CreateFragmentArguments(parameters);
                fragment.Arguments = args;
            }

            appActivity.SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.fragment_container, fragment)
                .AddToBackStack(typeof(TFragment).Name)
                .Commit();
        }

        return Task.CompletedTask;
    }

    private Task NavigateToActivity<TActivity>(object? parameters) where TActivity : Activity
    {
        var currentActivity = _currentActivityProvider();
        var intent = new Intent(currentActivity, typeof(TActivity));

        if (parameters != null)
        {
            // Serialize parameters if needed
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
            intent.PutExtra("parameters", json);
        }

        currentActivity.StartActivity(intent);
        return Task.CompletedTask;
    }

    private string GetRouteFromViewModelType(Type viewModelType)
    {
        return viewModelType.Name switch
        {
            nameof(AddTransactionViewModel) => "AddTransaction",
            nameof(SettingsViewModel) => "Settings",
            _ => viewModelType.Name.Replace("ViewModel", "")
        };
    }

    private Bundle CreateFragmentArguments(object parameters)
    {
        var bundle = new Bundle();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);

        if (parameters is TransactionDto)
        {
            bundle.PutString("transaction_json", json);
        }
        else
        {
            bundle.PutString("parameters", json);
        }

        return bundle;
    }
}
