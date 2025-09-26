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
/// Extensions to configure dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every Infrastructure service.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Database
        services.AddDatabase(connectionString);

        // Repositories
        services.AddRepositories();

        // Domain services
        services.AddDomainServices();

        // Application services
        services.AddApplicationServices();

        // Validators
        services.AddValidators();

        // AutoMapper
        services.AddAutoMapper(typeof(Application.Mappings.MappingProfile));

        // External services
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
        // Register repositories as Scoped (one instance per request/operation)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<ITransactionService, TransactionDomainService>();
        services.AddScoped<MoneyCalculatorService>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<TransactionAppService>();
        services.AddScoped<CategoryAppService>();

        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateTransactionValidator>();

        // Register specific validators
        services.AddScoped<CreateTransactionValidator>();
        services.AddScoped<CategoryValidator>();

        // ✅ Register these view models:
        services.AddScoped<TransactionListViewModel>();
        services.AddScoped<AddTransactionViewModel>();
        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        // HttpClient for external APIs
        services.AddHttpClient<ApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "MoneyTracker/1.0");
        });

        return services;
    }
}