using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.ValueObjects;

namespace MoneyTracker.Infrastructure.Database;

/// <summary>
/// Helper para inicialización y migraciones de base de datos
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Inicializa la base de datos con datos de ejemplo para desarrollo
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
    /// Crea transacciones de ejemplo para desarrollo
    /// </summary>
    private static async Task CreateSampleTransactionsAsync(MoneyTrackerContext context)
    {
        // Obtener categorías existentes
        var categories = await context.Categories.ToListAsync();
        if (!categories.Any()) return;

        var alimentacionId = categories.FirstOrDefault(c => c.Name == "Alimentación")?.Id ?? 1;
        var transporteId = categories.FirstOrDefault(c => c.Name == "Transporte")?.Id ?? 2;
        var ingresosId = categories.FirstOrDefault(c => c.Name == "Ingresos")?.Id ?? 7;

        var sampleTransactions = new List<Transaction>
            {
                // Ingresos
                Transaction.CreateIncome("Salario Enero", new Money(3000, "USD"), ingresosId, DateTime.Now.AddDays(-25)),
                Transaction.CreateIncome("Freelance", new Money(500, "USD"), ingresosId, DateTime.Now.AddDays(-20)),
                
                // Gastos
                Transaction.CreateExpense("Supermercado", new Money(150, "USD"), alimentacionId, DateTime.Now.AddDays(-5)),
                Transaction.CreateExpense("Gasolina", new Money(80, "USD"), transporteId, DateTime.Now.AddDays(-3)),
                Transaction.CreateExpense("Restaurante", new Money(45, "USD"), alimentacionId, DateTime.Now.AddDays(-2)),
                Transaction.CreateExpense("Uber", new Money(25, "USD"), transporteId, DateTime.Now.AddDays(-1)),
            };

        context.Transactions.AddRange(sampleTransactions);
        await context.SaveChangesAsync();

        System.Diagnostics.Debug.WriteLine($"✅ Created {sampleTransactions.Count} sample transactions");
    }

    /// <summary>
    /// Verifica la salud de la base de datos
    /// </summary>
    public static async Task<bool> CheckDatabaseHealthAsync(MoneyTrackerContext context)
    {
        try
        {
            // Verificar conexión
            await context.Database.CanConnectAsync();

            // Verificar que las tablas existen
            var categoriesExist = await context.Categories.AnyAsync();

            // Verificar integridad básica
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
    /// Resetea la base de datos (solo para desarrollo)
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