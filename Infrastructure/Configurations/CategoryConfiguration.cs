using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Category
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Configuración de tabla
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        // Propiedades principales
        builder.Property(c => c.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Nombre de la categoría");

        builder.Property(c => c.Description)
            .HasMaxLength(200)
            .HasComment("Descripción de la categoría");

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7) // #RRGGBB
            .HasDefaultValue("#2196F3")
            .HasComment("Color en formato hexadecimal");

        builder.Property(c => c.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("category")
            .HasComment("Nombre del icono");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indica si la categoría está activa");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Fecha de creación");

        // Restricciones únicas
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Category_Name_Unique");

        // Índices para performance
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Category_IsActive");

        // Relación con Transactions configurada desde TransactionConfiguration
    }
}