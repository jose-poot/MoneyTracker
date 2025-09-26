namespace MoneyTracker.Presentation.Extensions;

public class ActionDisposable : IDisposable
{
    private readonly Action _action;
    private volatile bool _disposed;

    public ActionDisposable(Action action) => _action = action;

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ActionDisposable: {ex.Message}");
            }
        }
    }
}