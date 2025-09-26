using Android.App;
using Android.Content;
using Android.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Services;
using MoneyTracker.Infrastructure.Database;
using MoneyTracker.Infrastructure.DependencyInjection;
using MoneyTracker.Presentation.Services;
using MoneyTracker.Presentation.Services.Interfaces;
using System;
using System.IO;

namespace MoneyTracker;

/// <summary>
/// Application class that configures dependency injection for the app.
/// Runs when the Android application starts.
/// </summary>
[Application]
public class MoneyTrackerApplication : Android.App.Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    private CurrentActivityProvider? _currentActivityProvider;

    public MoneyTrackerApplication(IntPtr handle, JniHandleOwnership transer)
        : base(handle, transer)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();

        try
        {
            _currentActivityProvider = new CurrentActivityProvider();
            RegisterActivityLifecycleCallbacks(_currentActivityProvider);

            // Configure dependency injection
            ConfigureServices();

            // Initialize the database
            _ = InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            // Critical log - the app cannot continue without DI
            System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR initializing app: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Configures every service required by the application.
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Warning);
#endif
        });

        // Configure database path
        var dbPath = GetDatabasePath();
        var connectionString = $"Data Source={dbPath}";
 
        if (_currentActivityProvider == null)
        {
            throw new InvalidOperationException("The current activity provider has not been initialized.");
        }

        services.AddSingleton<Func<Activity>>(_ => () => _currentActivityProvider.GetCurrentActivity());
        services.AddSingleton<Func<Context>>(sp => () => sp.GetRequiredService<Func<Activity>>()());
        services.AddSingleton<IDialogService>(sp => new AndroidDialogService(sp.GetRequiredService<Func<Context>>()));
        services.AddSingleton<INavigationService>(sp => new AndroidNavigationService(sp.GetRequiredService<Func<Activity>>()));
        services.AddSingleton<ICacheService>(_ => new AndroidCacheService(ApplicationContext ?? throw new InvalidOperationException("ApplicationContext is not available")));
        services.AddSingleton<IMediaPickerService>(sp => new AndroidMediaPickerService(sp.GetRequiredService<Func<Activity>>()));
        // ViewModels
        services.AddTransient<MoneyTracker.Presentation.ViewModels.SettingsViewModel>();
        services.AddScoped<UserAppService>();
        // Register every Infrastructure service
        services.AddInfrastructure(connectionString);

        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
        MoneyTracker.Presentation.Binding.AppServices.Initialize(ServiceProvider);
        // Validate critical configuration
        ValidateServiceConfiguration();
    }

    /// <summary>
    /// Gets the path where the database will be stored.
    /// </summary>
    private string GetDatabasePath()
    {
        try
        {
            // On Android use the app's external files directory
            var externalFilesDir = GetExternalFilesDir(null);
            if (externalFilesDir != null)
            {
                return Path.Combine(externalFilesDir.AbsolutePath, "MoneyTracker.db");
            }

            // Fallback: use the internal directory
            var internalDir = FilesDir;
            return Path.Combine(internalDir!.AbsolutePath, "MoneyTracker.db");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting database path: {ex.Message}");
            // Final fallback
            return "/data/data/com.moneytracker.app/files/MoneyTracker.db";
        }
    }

    /// <summary>
    /// Validates that critical services are registered correctly.
    /// </summary>
    private void ValidateServiceConfiguration()
    {
        try
        {
            // Verify critical services
            _ = ServiceProvider!.GetRequiredService<MoneyTrackerContext>();
            _ = ServiceProvider.GetRequiredService<TransactionAppService>();
            _ = ServiceProvider.GetRequiredService<CategoryAppService>();
            _ = ServiceProvider.GetRequiredService<IDialogService>();
            _ = ServiceProvider.GetRequiredService<INavigationService>();
            _ = ServiceProvider.GetRequiredService<ICacheService>();
            _ = ServiceProvider.GetRequiredService<IMediaPickerService>();

            System.Diagnostics.Debug.WriteLine("✅ Service configuration validated successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Service configuration validation failed: {ex.Message}");
            throw new InvalidOperationException("Failed to configure services", ex);
        }
    }

    /// <summary>
    /// Initializes the database with migrations and seed data.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = ServiceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MoneyTrackerContext>();
            var categoryService = scope.ServiceProvider.GetRequiredService<CategoryAppService>();

            // Create the database if it does not exist
            await context.Database.EnsureCreatedAsync();

            System.Diagnostics.Debug.WriteLine($"📁 Database initialized at: {context.Database.GetDbConnection().ConnectionString}");

            // Initialize default categories
            var categoriesCreated = await categoryService.InitializeDefaultCategoriesAsync();
            if (categoriesCreated > 0)
            {
                System.Diagnostics.Debug.WriteLine($"📂 Created {categoriesCreated} default categories");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database initialization failed: {ex.Message}");
            // Do not throw here - the app can continue and retry later
        }
    }

    /// <summary>
    /// Static helper to resolve services from anywhere.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        if (ServiceProvider == null)
            throw new InvalidOperationException("ServiceProvider has not been initialized");

        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Performs cleanup when the application shuts down.
    /// </summary>
    public override void OnTerminate()
    {
        try
        {
            // Close database connections
            using var scope = ServiceProvider?.CreateScope();
            var context = scope?.ServiceProvider.GetService<MoneyTrackerContext>();
            context?.Dispose();

            if (_currentActivityProvider != null)
            {
                UnregisterActivityLifecycleCallbacks(_currentActivityProvider);
            }

            // Dispose the ServiceProvider
            if (ServiceProvider is IDisposable disposable)
                disposable.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during app termination: {ex.Message}");
        }
        finally
        {
            base.OnTerminate();
        }
    }
}
