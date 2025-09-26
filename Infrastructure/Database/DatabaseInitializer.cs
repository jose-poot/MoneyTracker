using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Infrastructure.Database;

/// <summary>
/// Helper for database initialization and migrations.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the database with sample data for development.
    /// </summary>
    public static async Task SeedDevelopmentDataAsync(MoneyTrackerContext context, bool includeSampleTransactions = true)
    {
        try
        {
            if (includeSampleTransactions && !await context.Transactions.AnyAsync())
            {
                await CreateSampleTransactionsAsync(context);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error seeding development data: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates sample transactions for development.
    /// </summary>
    private static async Task CreateSampleTransactionsAsync(MoneyTrackerContext context)
    {
        // Retrieve existing categories
        var categories = await context.Categories.ToListAsync();
        if (!categories.Any()) return;

        var foodCategoryId = categories.FirstOrDefault(c => c.Name == "Food")?.Id ?? 1;
        var transportCategoryId = categories.FirstOrDefault(c => c.Name == "Transport")?.Id ?? 2;
        var incomeCategoryId = categories.FirstOrDefault(c => c.Name == "Income")?.Id ?? 7;

        var sampleTransactions = new List<Transaction>
            {
                // Income
                Transaction.CreateIncome("January Salary", new Money(3000, "USD"), incomeCategoryId, DateTime.Now.AddDays(-25)),
                Transaction.CreateIncome("Freelance", new Money(500, "USD"), incomeCategoryId, DateTime.Now.AddDays(-20)),

                // Expenses
                Transaction.CreateExpense("Groceries", new Money(150, "USD"), foodCategoryId, DateTime.Now.AddDays(-5)),
                Transaction.CreateExpense("Gasoline", new Money(80, "USD"), transportCategoryId, DateTime.Now.AddDays(-3)),
                Transaction.CreateExpense("Restaurant", new Money(45, "USD"), foodCategoryId, DateTime.Now.AddDays(-2)),
                Transaction.CreateExpense("Ride Share", new Money(25, "USD"), transportCategoryId, DateTime.Now.AddDays(-1)),
            };

        context.Transactions.AddRange(sampleTransactions);
        await context.SaveChangesAsync();

        System.Diagnostics.Debug.WriteLine($"✅ Created {sampleTransactions.Count} sample transactions");
    }

    /// <summary>
    /// Checks the health of the database.
    /// </summary>
    public static async Task<bool> CheckDatabaseHealthAsync(MoneyTrackerContext context)
    {
        try
        {
            // Verify connectivity
            await context.Database.CanConnectAsync();

            // Verify that tables exist
            var categoriesExist = await context.Categories.AnyAsync();

            // Verify basic integrity
            var transactionCount = await context.Transactions.CountAsync();
            var categoryCount = await context.Categories.CountAsync();

            System.Diagnostics.Debug.WriteLine($"📊 Database Health: {transactionCount} transactions, {categoryCount} categories");

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database health check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Resets the database (development only).
    /// </summary>
    public static async Task ResetDatabaseAsync(MoneyTrackerContext context)
    {
#if DEBUG
        try
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            System.Diagnostics.Debug.WriteLine("🔄 Database reset completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Database reset failed: {ex.Message}");
        }
#else
            throw new InvalidOperationException("Database reset is only available in debug mode");
#endif
    }
}