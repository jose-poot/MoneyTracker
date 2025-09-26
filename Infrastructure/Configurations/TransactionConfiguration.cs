using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for the Transaction entity.
/// Defines how it maps to the database.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Table configuration
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // Property configuration
        builder.Property(t => t.Id)
            .IsRequired()
            .ValueGeneratedOnAdd(); // Auto-increment

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Transaction description");

        // Special configuration for the Money value object.
        // EF does not handle Value Objects natively, so we flatten them.
        builder.Property(t => t.AmountValue)
            .IsRequired()
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .HasComment("Transaction amount");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .HasComment("ISO currency code");

        // Store the enum as a string for readability
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Transaction type: Income or Expense");

        // Date configuration
        builder.Property(t => t.Date)
            .IsRequired()
            .HasColumnType("datetime")
            .HasComment("Transaction date");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Record creation date");

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime")
            .HasComment("Last modification date");

        // Optional properties
        builder.Property(t => t.Notes)
            .HasMaxLength(500)
            .HasComment("Additional notes");

        builder.Property(t => t.Location)
            .HasMaxLength(100)
            .HasComment("Transaction location");

        builder.Property(t => t.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates whether the transaction is recurring");

        // Relationship with Category
        builder.Property(t => t.CategoryId)
            .IsRequired()
            .HasComment("Category ID");

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict) // Do not delete a category that has transactions
            .HasConstraintName("FK_Transaction_Category");

        // Indexes to improve performance
        builder.HasIndex(t => t.Date)
            .HasDatabaseName("IX_Transaction_Date");

        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_Transaction_CategoryId");

        builder.HasIndex(t => t.Type)
            .HasDatabaseName("IX_Transaction_Type");

        builder.HasIndex(t => new { t.Date, t.Type })
            .HasDatabaseName("IX_Transaction_Date_Type");

        // Ignore calculated properties that are not stored in the database
        builder.Ignore(t => t.Amount); // AmountValue and Currency are used instead
    }
}