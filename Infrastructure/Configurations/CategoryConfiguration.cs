using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for the Category entity.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table configuration
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        // Primary properties
        builder.Property(c => c.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Category name");

        builder.Property(c => c.Description)
            .HasMaxLength(200)
            .HasComment("Category description");

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7) // #RRGGBB
            .HasDefaultValue("#2196F3")
            .HasComment("Hexadecimal color value");

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("category")
            .HasComment("Icon name");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates whether the category is active");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Creation date");

        // Unique constraints
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Category_Name_Unique");

        // Indexes for performance
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Category_IsActive");

        // Relationship with Transactions configured in TransactionConfiguration
    }
}