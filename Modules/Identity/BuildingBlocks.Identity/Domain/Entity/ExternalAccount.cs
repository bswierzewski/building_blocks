using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Domain.Entity;

/// <summary>
/// Represents a link between a local user and an external identity provider account.
/// </summary>
public sealed class ExternalAccount : AuditableEntity<Guid>
{
    public ExternalProvider Provider { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private ExternalAccount() { }

    private ExternalAccount(Guid id, Guid userId, ExternalProvider provider, string externalId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        Id = id;
        UserId = userId;
        Provider = provider;
        ExternalId = externalId.Trim();
    }

    /// <summary>
    /// Creates a new external account link for the specified user.
    /// </summary>
    public static ExternalAccount Create(Guid userId, ExternalProvider provider, string externalId)
        => new(Guid.CreateVersion7(), userId, provider, externalId);
}