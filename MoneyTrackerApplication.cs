using Android.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Services;
using MoneyTracker.Infrastructure.Database;
using MoneyTracker.Infrastructure.DependencyInjection;
using System;
using System.IO;

namespace MoneyTracker;

/// <summary>
/// Clase Application que configura toda la inyección de dependencias
/// Se ejecuta cuando inicia la aplicación Android
/// </summary>
[Application]
public class MoneyTrackerApplication : Android.App.Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public MoneyTrackerApplication(IntPtr handle, JniHandleOwnership transer)
        : base(handle, transer)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();

        try
        {
            // Configurar inyección de dependencias
            ConfigureServices();

            // Inicializar base de datos
            _ = InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            // Log crítico - la app no puede continuar sin DI
            System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR initializing app: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Configura todos los servicios de la aplicación
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

        // Configurar ruta de base de datos
        var dbPath = GetDatabasePath();
        var connectionString = $"Data Source={dbPath}";
        // Servicios
        services.AddSingleton<MoneyTracker.Presentation.Navigation.INavigator, MoneyTracker.Presentation.Navigation.Navigator>();

        // ViewModels
        services.AddTransient<MoneyTracker.Presentation.ViewModels.SettingsViewModel>();
        services.AddScoped<UserAppService>();
        // Registrar todos los servicios de Infrastructure
        services.AddInfrastructure(connectionString);

        // Construir el service provider
        ServiceProvider = services.BuildServiceProvider();
        MoneyTracker.Presentation.Binding.AppServices.Initialize(ServiceProvider);
        // Validar configuración crítica
        ValidateServiceConfiguration();
    }

    /// <summary>
    /// Obtiene la ruta donde guardar la base de datos
    /// </summary>
    private string GetDatabasePath()
    {
        try
        {
            // En Android, usar el directorio de archivos externos de la app
            var externalFilesDir = GetExternalFilesDir(null);
            if (externalFilesDir != null)
            {
                return Path.Combine(externalFilesDir.AbsolutePath, "MoneyTracker.db");
            }

            // Fallback: directorio interno
            var internalDir = FilesDir;
            return Path.Combine(internalDir!.AbsolutePath, "MoneyTracker.db");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting database path: {ex.Message}");
            // Fallback absoluto
            return "/data/data/com.moneytracker.app/files/MoneyTracker.db";
        }
    }

    /// <summary>
    /// Valida que los servicios críticos estén registrados correctamente
    /// </summary>
    private void ValidateServiceConfiguration()
    {
        try
        {
            // Verificar servicios críticos
            _ = ServiceProvider!.GetRequiredService<MoneyTrackerContext>();
            _ = ServiceProvider.GetRequiredService<TransactionAppService>();
            _ = ServiceProvider.GetRequiredService<CategoryAppService>();

            System.Diagnostics.Debug.WriteLine("✅ Service configuration validated successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Service configuration validation failed: {ex.Message}");
            throw new InvalidOperationException("Failed to configure services", ex);
        }
    }

    /// <summary>
    /// Inicializa la base de datos con migraciones y datos iniciales
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = ServiceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MoneyTrackerContext>();
            var categoryService = scope.ServiceProvider.GetRequiredService<CategoryAppService>();

            // Crear base de datos si no existe
            await context.Database.EnsureCreatedAsync();

            System.Diagnostics.Debug.WriteLine($"📁 Database initialized at: {context.Database.GetDbConnection().ConnectionString}");

            // Inicializar categorías predeterminadas
            var categoriesCreated = await categoryService.InitializeDefaultCategoriesAsync();
            if (categoriesCreated > 0)
            {
                System.Diagnostics.Debug.WriteLine($"📂 Created {categoriesCreated} default categories");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database initialization failed: {ex.Message}");
            // No hacer throw aquí - la app puede continuar y reintentar más tarde
        }
    }

    /// <summary>
    /// Método estático para obtener servicios desde cualquier lugar
    /// </summary>
    public static T GetService<T>() where T : class
    {
        if (ServiceProvider == null)
            throw new InvalidOperationException("ServiceProvider no está inicializado");

        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Limpieza cuando la aplicación se cierra
    /// </summary>
    public override void OnTerminate()
    {
        try
        {
            // Cerrar conexiones de base de datos
            using var scope = ServiceProvider?.CreateScope();
            var context = scope?.ServiceProvider.GetService<MoneyTrackerContext>();
            context?.Dispose();

            // Dispose del ServiceProvider
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
