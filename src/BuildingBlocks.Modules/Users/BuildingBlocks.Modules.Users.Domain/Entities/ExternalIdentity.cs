using BuildingBlocks.Modules.Users.Domain.Enums;
using BuildingBlocks.Domain.Primitives;

namespace BuildingBlocks.Modules.Users.Domain.Entities;

/// <summary>
/// Represents an external identity linked to a user from an authentication provider.
/// Allows a single user to authenticate through multiple providers.
/// </summary>
public class ExternalIdentity : Entity<Guid>
{
    /// <summary>
    /// Gets the identity provider that authenticated this identity.
    /// </summary>
    public IdentityProvider Provider { get; private set; }

    /// <summary>
    /// Gets the user ID from the external provider (e.g., Auth0 subject claim).
    /// This is the unique identifier from the JWT token's 'sub' claim.
    /// </summary>
    public string ExternalUserId { get; private set; } = null!;

    /// <summary>
    /// Gets optional metadata from the provider stored as JSON.
    /// Can include additional provider-specific information.
    /// </summary>
    public string? ProviderMetadata { get; private set; }

    /// <summary>
    /// Gets the date and time when this identity was linked to the user.
    /// </summary>
    public DateTimeOffset LinkedAt { get; private set; }

    private ExternalIdentity() { } // EF Core

    /// <summary>
    /// Creates a new external identity link.
    /// </summary>
    public static ExternalIdentity Create(
        IdentityProvider provider,
        string externalUserId,
        string? providerMetadata = null)
    {
        if (provider == IdentityProvider.Unknown)
            throw new ArgumentException("Cannot create identity with Unknown provider", nameof(provider));

        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        return new ExternalIdentity
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            ExternalUserId = externalUserId,
            ProviderMetadata = providerMetadata,
            LinkedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Updates provider metadata.
    /// </summary>
    public void UpdateMetadata(string? metadata)
    {
        ProviderMetadata = metadata;
    }
}
