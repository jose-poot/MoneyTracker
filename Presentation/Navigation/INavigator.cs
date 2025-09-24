namespace MoneyTracker.Presentation.Navigation;
public interface INavigator
{
    void GoTo<TActivity>(Activity from, Bundle? args = null) where TActivity : Activity;
    void GoBack(Activity current);
}
