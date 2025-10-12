using BuildingBlocks.Modules.Users.Domain.Entities;
using BuildingBlocks.Modules.Users.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the ExternalIdentity entity.
/// </summary>
public class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
{
    /// <summary>
    /// Configures the ExternalIdentity entity mapping.
    /// </summary>
    public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(x => x.ExternalUserId)
            .IsRequired()
            .HasMaxLength(500); // Some providers have long IDs

        builder.Property(x => x.ProviderMetadata)
            .HasMaxLength(2000); // JSON metadata

        builder.Property(x => x.LinkedAt)
            .IsRequired();

        // Unique constraint: one provider+external ID combination can only exist once
        builder.HasIndex(x => new { x.Provider, x.ExternalUserId })
            .IsUnique()
            .HasDatabaseName("IX_ExternalIdentity_Provider_ExternalUserId");

        // Index for lookups by provider
        builder.HasIndex(x => x.Provider);

        builder.ToTable("ExternalIdentities");
    }
}
