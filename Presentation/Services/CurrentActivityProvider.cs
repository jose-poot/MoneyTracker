using Android.App;
using Android.OS;
using System;
using Application = Android.App.Application;

namespace MoneyTracker.Presentation.Services;

public class CurrentActivityProvider : Java.Lang.Object, Application.IActivityLifecycleCallbacks
{
    private readonly object _syncRoot = new();
    private Activity? _currentActivity;

    public Activity GetCurrentActivity()
    {
        var activity = CurrentActivity;
        if (activity == null)
        {
            throw new InvalidOperationException("No hay una actividad actual disponible.");
        }

        return activity;
    }

    public Activity? CurrentActivity
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentActivity;
            }
        }
    }

    public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
    {
        SetCurrentActivity(activity);
    }

    public void OnActivityDestroyed(Activity activity)
    {
        ClearActivity(activity);
    }

    public void OnActivityPaused(Activity activity)
    {
    }

    public void OnActivityResumed(Activity activity)
    {
        SetCurrentActivity(activity);
    }

    public void OnActivitySaveInstanceState(Activity activity, Bundle? outState)
    {
    }

    public void OnActivityStarted(Activity activity)
    {
        SetCurrentActivity(activity);
    }

    public void OnActivityStopped(Activity activity)
    {
        ClearActivity(activity, onlyIfSame: true);
    }

    private void SetCurrentActivity(Activity activity)
    {
        lock (_syncRoot)
        {
            _currentActivity = activity;
        }
    }

    private void ClearActivity(Activity activity, bool onlyIfSame = false)
    {
        lock (_syncRoot)
        {
            if (!onlyIfSame || ReferenceEquals(_currentActivity, activity))
            {
                _currentActivity = null;
            }
        }
    }
}
