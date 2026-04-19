using BuildingBlocks.Identity.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Identity.Infrastructure.Persistence.Configurations;

internal sealed class ExternalAccountConfiguration : IEntityTypeConfiguration<ExternalAccount>
{
    public void Configure(EntityTypeBuilder<ExternalAccount> builder)
    {
        builder.Property(x => x.ExternalId)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ExternalId })
            .IsUnique();
    }
}