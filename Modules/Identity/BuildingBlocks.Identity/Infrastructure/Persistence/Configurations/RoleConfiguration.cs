using BuildingBlocks.Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property<HashSet<string>>("_permissions")
            .HasColumnName("Permissions")
            .HasColumnType("text[]")
            .HasConversion(
                permissions => permissions.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                values => values.ToHashSet(StringComparer.Ordinal),
                PermissionsComparer)
            .IsRequired();

        builder.Ignore(x => x.Permissions);

        builder.Navigation(x => x.Users)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }

    private static readonly ValueComparer<HashSet<string>> PermissionsComparer = new(
        (left, right) => HaveSamePermissions(left, right),
        set => GetPermissionsHashCode(set),
        set => set.ToHashSet(StringComparer.Ordinal));

    private static bool HaveSamePermissions(HashSet<string>? left, HashSet<string>? right)
        => left == right || left is not null && right is not null && left.SetEquals(right);

    private static int GetPermissionsHashCode(HashSet<string>? permissions)
        => permissions is null
            ? 0
            : permissions
                .OrderBy(value => value, StringComparer.Ordinal)
                .Aggregate(0, (hash, permission) => HashCode.Combine(hash, StringComparer.Ordinal.GetHashCode(permission)));
}