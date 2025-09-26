using Android.App;
using System;

namespace MoneyTracker.Presentation.Services;

public static class CurrentActivityHolder
{
    private static readonly object _sync = new();
    private static WeakReference<Activity>? _current;

    public static Activity? Current
    {
        get
        {
            lock (_sync)
            {
                if (_current != null && _current.TryGetTarget(out var activity))
                {
                    return activity;
                }

                return null;
            }
        }
    }

    public static void Register(Activity activity)
    {
        if (activity == null)
        {
            return;
        }

        lock (_sync)
        {
            _current = new WeakReference<Activity>(activity);
        }
    }

    public static void Unregister(Activity activity)
    {
        if (activity == null)
        {
            return;
        }

        lock (_sync)
        {
            if (_current != null && _current.TryGetTarget(out var existing) && ReferenceEquals(existing, activity))
            {
                _current = null;
            }
        }
    }
}
