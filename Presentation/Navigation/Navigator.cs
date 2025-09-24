using Android.Content;

namespace MoneyTracker.Presentation.Navigation;

public sealed class Navigator : INavigator
{
    public void GoTo<TActivity>(Activity from, Bundle? args = null) where TActivity : Activity
    {
        var intent = new Intent(from, typeof(TActivity));
        if (args is not null)
            intent.PutExtras(args);

        from.StartActivity(intent);
    }

    public void GoBack(Activity current)
    {
        current.Finish();
    }
}