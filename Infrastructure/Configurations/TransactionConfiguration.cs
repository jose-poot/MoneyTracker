using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Transaction
/// Define cómo se mapea a la base de datos
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Configuración de tabla
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // Configuración de propiedades
        builder.Property(t => t.Id)
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Descripción de la transacción");

        // Configuración especial para Value Object Money
        // EF no maneja Value Objects nativamente, así que los "aplanamos"
        builder.Property(t => t.AmountValue)
            .IsRequired()
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .HasComment("Monto de la transacción");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .HasComment("Código de moneda ISO");

        // Enum como string para facilitar lectura
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Tipo de transacción: Income o Expense");

        // Fechas con configuración específica
        builder.Property(t => t.Date)
            .IsRequired()
            .HasColumnType("datetime")
            .HasComment("Fecha de la transacción");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Fecha de creación del registro");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime")
            .HasComment("Fecha de última modificación");

        // Propiedades opcionales
        builder.Property(t => t.Notes)
            .HasMaxLength(500)
            .HasComment("Notas adicionales");

        builder.Property(t => t.Location)
            .HasMaxLength(100)
            .HasComment("Ubicación de la transacción");

        builder.Property(t => t.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indica si es una transacción recurrente");

        // Relación con Category
        builder.Property(t => t.CategoryId)
            .IsRequired()
            .HasComment("ID de la categoría");

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict) // No borrar categoría si tiene transacciones
            .HasConstraintName("FK_Transaction_Category");

        // Índices para mejorar performance
        builder.HasIndex(t => t.Date)
            .HasDatabaseName("IX_Transaction_Date");

        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_Transaction_CategoryId");

        builder.HasIndex(t => t.Type)
            .HasDatabaseName("IX_Transaction_Type");

        builder.HasIndex(t => new { t.Date, t.Type })
            .HasDatabaseName("IX_Transaction_Date_Type");

        // Ignorar propiedades calculadas que no van a la BD
        builder.Ignore(t => t.Amount); // Usamos AmountValue y Currency en su lugar
    }
}