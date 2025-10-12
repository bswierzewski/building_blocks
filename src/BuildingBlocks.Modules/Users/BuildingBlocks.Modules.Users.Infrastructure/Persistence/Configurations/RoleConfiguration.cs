using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Configures the Role entity mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.ModuleName)
            .IsRequired()
            .HasMaxLength(100);

        // Many-to-many relationship: Role <-> Permission
        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RolePermission",
                j => j.HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey("PermissionId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("RoleId", "PermissionId");
                    j.ToTable("RolePermissions");
                    j.HasIndex("PermissionId");
                });

        // Indexes
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Role_Name");

        builder.HasIndex(r => r.ModuleName)
            .HasDatabaseName("IX_Role_ModuleName");

        builder.ToTable("Roles");
    }
}
