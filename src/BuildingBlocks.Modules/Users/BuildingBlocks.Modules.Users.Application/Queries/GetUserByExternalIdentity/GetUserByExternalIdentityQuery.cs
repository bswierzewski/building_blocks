using BuildingBlocks.Modules.Users.Domain.Enums;
using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserByExternalIdentity;

/// <summary>
/// Query to retrieve a user by their external identity (provider + external user ID).
/// Used by middleware and inter-module communication.
/// </summary>
public record GetUserByExternalIdentityQuery(
    IdentityProvider Provider,
    string ExternalUserId) : IRequest<GetUserByExternalIdentityDto?>;

/// <summary>
/// DTO representing a user retrieved by external identity.
/// </summary>
public record GetUserByExternalIdentityDto
{
    /// <summary>
    /// Gets the internal user ID (Guid).
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets whether the user account is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the date and time of the user's last login.
    /// </summary>
    public DateTimeOffset LastLoginAt { get; init; }

    /// <summary>
    /// Gets the list of role IDs assigned to this user.
    /// </summary>
    public Guid[] RoleIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the list of permission IDs granted to this user through their roles.
    /// </summary>
    public Guid[] PermissionIds { get; init; } = Array.Empty<Guid>();
}
