namespace MoneyTracker.Presentation.Extensions;

public class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;
    private volatile bool _disposed;

    public CompositeDisposable(params IDisposable[] disposables)
    {
        _disposables = new List<IDisposable>(disposables);
    }

    public void Add(IDisposable disposable)
    {
        if (_disposed)
        {
            disposable?.Dispose();
            return;
        }

        lock (_disposables)
        {
            if (_disposed)
            {
                disposable?.Dispose();
                return;
            }
            _disposables.Add(disposable);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        IDisposable[] toDispose;
        lock (_disposables)
        {
            toDispose = _disposables.ToArray();
            _disposables.Clear();
        }

        foreach (var disposable in toDispose)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing: {ex.Message}");
            }
        }
    }
}