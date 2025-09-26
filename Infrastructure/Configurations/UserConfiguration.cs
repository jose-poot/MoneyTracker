using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Core.Entities;

namespace MoneyTracker.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for the User entity.
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
            .HasComment("User name");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("User email");

        builder.Property(u => u.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .HasComment("Preferred currency");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime")
            .HasDefaultValueSql("datetime('now')");

        builder.Property(u => u.LastLoginAt)
            .HasColumnType("datetime");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // User configuration
        builder.Property(u => u.ShowNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.DateFormat)
            .HasMaxLength(20)
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(u => u.Theme)
            .HasMaxLength(20)
            .HasDefaultValue("Light");

        // Unique constraint on email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email_Unique");
    }
}