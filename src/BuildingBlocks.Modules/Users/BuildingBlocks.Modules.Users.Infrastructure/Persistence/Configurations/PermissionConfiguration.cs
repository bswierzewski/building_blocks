using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Permission entity.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Configures the Permission entity mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.ModuleName)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("IX_Permission_Name");

        builder.HasIndex(p => p.ModuleName)
            .HasDatabaseName("IX_Permission_ModuleName");

        builder.ToTable("Permissions");
    }
}
