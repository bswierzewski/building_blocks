using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasMany(x => x.ExternalAccounts)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.Navigation(x => x.ExternalAccounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Roles)
            .WithMany(x => x.Users);

        builder.Navigation(x => x.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}