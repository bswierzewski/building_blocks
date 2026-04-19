using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Domain.Entity;

/// <summary>
/// Represents a link between a local user and an external identity provider account.
/// </summary>
public sealed class ExternalAccount : Entity<Guid>
{
    public ExternalProvider Provider { get; private set; }

    public string Subject { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private ExternalAccount() { }

    private ExternalAccount(Guid id, Guid userId, ExternalProvider provider, string subject)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        Id = id;
        UserId = userId;
        Provider = provider;
        Subject = subject.Trim();
    }

    /// <summary>
    /// Creates a new external account link for the specified user.
    /// </summary>
    public static ExternalAccount Create(Guid userId, ExternalProvider provider, string subject)
        => new(Guid.CreateVersion7(), userId, provider, subject);
}