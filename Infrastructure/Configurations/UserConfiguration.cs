using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad User
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Nombre del usuario");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Email del usuario");

        builder.Property(u => u.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .HasComment("Moneda preferida del usuario");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')");

        builder.Property(u => u.LastLoginAt)
            .HasColumnType("datetime");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Configuraciones de usuario
        builder.Property(u => u.ShowNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.DateFormat)
            .HasMaxLength(20)
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(u => u.Theme)
            .HasMaxLength(20)
            .HasDefaultValue("Light");

        // Restricción única en email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email_Unique");
    }
}