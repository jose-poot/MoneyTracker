using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Application.Services;
using MoneyTracker.Application.Validators;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Core.Interfaces.Services;
using MoneyTracker.Core.Services;
using MoneyTracker.Infrastructure.Api;
using MoneyTracker.Infrastructure.Database;
using MoneyTracker.Infrastructure.Repositories;
using MoneyTracker.Infrastructure.Services;
using MoneyTracker.Presentation.ViewModels;
namespace MoneyTracker.Infrastructure.DependencyInjection;

/// <summary>
/// Extensiones para configurar la inyección de dependencias
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios de Infrastructure
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Base de datos
        services.AddDatabase(connectionString);

        // Repositorios
        services.AddRepositories();

        // Servicios de dominio
        services.AddDomainServices();

        // Servicios de aplicación
        services.AddApplicationServices();

        // Validadores
        services.AddValidators();

        // AutoMapper
        services.AddAutoMapper(typeof(Application.Mappings.MappingProfile));

        // Servicios externos
        services.AddExternalServices();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MoneyTrackerContext>(options =>
        {
            options.UseSqlite(connectionString);

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar repositorios como Scoped (una instancia por request/operación)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Servicios de dominio
        services.AddScoped<ITransactionService, TransactionDomainService>();
        services.AddScoped<MoneyCalculatorService>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Servicios de aplicación
        services.AddScoped<TransactionAppService>();
        services.AddScoped<CategoryAppService>();

        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateTransactionValidator>();

        // Registrar validadores específicos
        services.AddScoped<CreateTransactionValidator>();
        services.AddScoped<CategoryValidator>();
        services.AddScoped<TransactionAppService>();
        services.AddScoped<CategoryAppService>();

        // ✅ AGREGAR ESTOS VIEWMODELS:
        services.AddScoped<TransactionListViewModel>();
        services.AddScoped<AddTransactionViewModel>();
        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        // HttpClient para APIs externas
        services.AddHttpClient<ApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "MoneyTracker/1.0");
        });

        return services;
    }
}