using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetCurrentUser;

/// <summary>
/// Query to retrieve the current authenticated user with their roles and permissions.
/// Uses the user ID from the IUser service (injected from claims).
/// </summary>
public record GetCurrentUserQuery() : IRequest<CurrentUserDto?>;

/// <summary>
/// DTO representing the current authenticated user with their roles and permissions.
/// Used by frontend (Next.js) to manage UI visibility and authorization.
/// </summary>
public record CurrentUserDto
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
    /// Gets the URL to the user's profile picture.
    /// </summary>
    public string? PictureUrl { get; init; }

    /// <summary>
    /// Gets whether the user account is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Gets the date and time of the user's last login.
    /// </summary>
    public DateTimeOffset LastLoginAt { get; init; }

    /// <summary>
    /// Gets the list of role names assigned to this user.
    /// </summary>
    public string[] Roles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of permission names granted to this user through their roles.
    /// </summary>
    public string[] Permissions { get; init; } = Array.Empty<string>();
}
