using Android.Content;
using MoneyTracker.Core.Entities;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Infrastructure.Database;

/// <summary>
/// Contexto de Entity Framework que representa nuestra base de datos
/// </summary>
public class MoneyTrackerContext : DbContext
{
    public MoneyTrackerContext(DbContextOptions<MoneyTrackerContext> options) : base(options)
    {
    }

    // DbSets - Representan las tablas en la base de datos
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Configuración del modelo de datos
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Datos iniciales (Seed Data)
        SeedInitialData(modelBuilder);
    }

    /// <summary>
    /// Configuración adicional de la conexión
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuración de fallback si no se configuró externamente
            var dbPath = Path.Combine(
                Android.App.Application.Context.GetExternalFilesDir(null)!.AbsolutePath,
                "MoneyTracker.db"
            );

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        // Configuraciones adicionales para desarrollo
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
    }

    /// <summary>
    /// Intercepta el SaveChanges para funcionalidad adicional
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar timestamps automáticamente
        UpdateTimestamps();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza CreatedAt/UpdatedAt automáticamente
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Transaction transaction)
            {
                if (entry.State == EntityState.Added)
                {
                    transaction.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    transaction.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entry.Entity is Category category && entry.State == EntityState.Added)
            {
                category.CreatedAt = DateTime.UtcNow;
            }

            if (entry.Entity is User user && entry.State == EntityState.Added)
            {
                user.CreatedAt = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Datos iniciales para la base de datos
    /// </summary>
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Usuario predeterminado
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Usuario Principal",
                Email = "usuario@moneytracker.com",
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );

        // Categorías predeterminadas
        var defaultCategories = Category.GetDefaultCategories();
        for (int i = 0; i < defaultCategories.Count; i++)
        {
            defaultCategories[i].Id = i + 1;
            defaultCategories[i].CreatedAt = DateTime.UtcNow;
        }

        modelBuilder.Entity<Category>().HasData(defaultCategories);
    }
}
