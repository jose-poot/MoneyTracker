namespace MoneyTracker.Presentation.Binding;

public static class AppServices
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;
    public static void Initialize(IServiceProvider provider) => ServiceProvider = provider;
}