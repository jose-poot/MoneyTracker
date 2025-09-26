using Android.Content;
using MoneyTracker.Core.Entities;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Infrastructure.Database;

/// <summary>
/// Entity Framework context that represents the application database.
/// </summary>
public class MoneyTrackerContext : DbContext
{
    public MoneyTrackerContext(DbContextOptions<MoneyTrackerContext> options) : base(options)
    {
    }

    // DbSets - Represent tables within the database
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Configures the data model.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply every configuration automatically
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed initial data
        SeedInitialData(modelBuilder);
    }

    /// <summary>
    /// Additional connection configuration.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback configuration when it was not provided externally
            var dbPath = Path.Combine(
                Android.App.Application.Context.GetExternalFilesDir(null)!.AbsolutePath,
                "MoneyTracker.db"
            );

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        // Additional settings for development
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
    }

    /// <summary>
    /// Intercepts SaveChanges for additional functionality.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        UpdateTimestamps();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates CreatedAt/UpdatedAt automatically.
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
    /// Seeds initial data for the database.
    /// </summary>
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Default user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Primary User",
                Email = "user@moneytracker.com",
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );

        // Default categories
        var defaultCategories = Category.GetDefaultCategories();
        for (int i = 0; i < defaultCategories.Count; i++)
        {
            defaultCategories[i].Id = i + 1;
            defaultCategories[i].CreatedAt = DateTime.UtcNow;
        }

        modelBuilder.Entity<Category>().HasData(defaultCategories);
    }
}
