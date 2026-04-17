using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Domain.Entities;

/// <summary>
/// Represents a link between a user and an external identity provider (e.g. Clerk, Auth0, Supabase).
/// A user may have multiple links — one per provider.
/// </summary>
public sealed class ExternalProvider : Entity<Guid>
{
    private ExternalProvider() { }

    /// <summary>The external identity provider type.</summary>
    public IdentityProvider Provider { get; private set; }

    /// <summary>The user's identifier on the provider side (<c>sub</c> claim).</summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>Creates a new link to an external identity provider.</summary>
    public static ExternalProvider Create(IdentityProvider provider, string subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return new ExternalProvider
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            Subject = subject.Trim()
        };
    }
}