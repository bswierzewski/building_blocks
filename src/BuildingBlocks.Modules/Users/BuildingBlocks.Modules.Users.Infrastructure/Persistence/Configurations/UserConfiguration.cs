using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the User aggregate.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the User entity mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        // Configure UserId value object conversion
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.CreateFrom(value))
            .ValueGeneratedNever();

        // Configure Email value object conversion
        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.LastLoginAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Auditable properties (inherited from AuditableEntity)
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.ModifiedAt);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        // Configure one-to-many relationship with ExternalIdentity
        builder.HasMany(u => u.ExternalIdentities)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many relationship: User <-> Role
        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserRole",
                j => j.HasOne<BuildingBlocks.Domain.Entities.Role>()
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("UserId", "RoleId");
                    j.ToTable("UserRoles");
                    j.HasIndex("RoleId");
                });

        // Indexes for performance
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.LastLoginAt);

        builder.ToTable("Users");
    }
}
