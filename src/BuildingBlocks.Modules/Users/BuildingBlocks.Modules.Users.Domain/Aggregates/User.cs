using BuildingBlocks.Modules.Users.Domain.Entities;
using BuildingBlocks.Modules.Users.Domain.Enums;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using BuildingBlocks.Domain.Primitives;
using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Modules.Users.Domain.Aggregates;

/// <summary>
/// User aggregate root representing an authenticated user in the system.
/// Maintains a unified internal ID regardless of external authentication providers.
/// </summary>
public class User : AuditableEntity<UserId>
{
    private readonly List<ExternalIdentity> _externalIdentities = new();
    private readonly List<Role> _roles = new();

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the date and time of the user's last login.
    /// </summary>
    public DateTimeOffset LastLoginAt { get; private set; }

    /// <summary>
    /// Gets whether the user account is active.
    /// Inactive users cannot authenticate.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the collection of external identities linked to this user.
    /// Allows authentication through multiple providers.
    /// </summary>
    public IReadOnlyCollection<ExternalIdentity> ExternalIdentities => _externalIdentities.AsReadOnly();

    /// <summary>
    /// Gets the collection of roles assigned to this user.
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    private User() { } // EF Core

    /// <summary>
    /// Creates a new user with the specified details.
    /// </summary>
    public static User Create(UserId id, Email email, string? displayName)
    {
        return new User
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            LastLoginAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// Links an external identity provider to this user.
    /// Idempotent - linking the same provider+externalUserId multiple times has no effect.
    /// </summary>
    public void LinkExternalIdentity(IdentityProvider provider, string externalUserId, string? metadata = null)
    {
        // Check if already linked
        if (_externalIdentities.Any(e => e.Provider == provider && e.ExternalUserId == externalUserId))
            return;

        var identity = ExternalIdentity.Create(provider, externalUserId, metadata);
        _externalIdentities.Add(identity);
    }

    /// <summary>
    /// Removes an external identity link from this user.
    /// </summary>
    public void UnlinkExternalIdentity(IdentityProvider provider, string externalUserId)
    {
        var identity = _externalIdentities.FirstOrDefault(e =>
            e.Provider == provider && e.ExternalUserId == externalUserId);

        if (identity != null)
        {
            _externalIdentities.Remove(identity);
        }
    }

    /// <summary>
    /// Updates the user's last login timestamp.
    /// Called by middleware on each authenticated request.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile(Email email, string? displayName)
    {
        Email = email;
        DisplayName = displayName;
    }

    /// <summary>
    /// Deactivates the user account.
    /// Deactivated users cannot authenticate.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates a previously deactivated user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Assigns a role to this user.
    /// Idempotent - assigning the same role multiple times has no effect.
    /// </summary>
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!_roles.Any(r => r.Id == role.Id))
        {
            _roles.Add(role);
        }
    }

    /// <summary>
    /// Removes a role from this user.
    /// </summary>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        _roles.RemoveAll(r => r.Id == role.Id);
    }

    /// <summary>
    /// Removes a role by its ID.
    /// </summary>
    public void RemoveRole(Guid roleId)
    {
        _roles.RemoveAll(r => r.Id == roleId);
    }

    /// <summary>
    /// Checks if this user has a specific role.
    /// </summary>
    public bool HasRole(string roleName)
    {
        return _roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this user has a specific permission (through their roles).
    /// </summary>
    public bool HasPermission(string permissionName)
    {
        return GetAllPermissions().Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all permissions from all roles assigned to this user.
    /// </summary>
    public IEnumerable<Permission> GetAllPermissions()
    {
        return _roles
            .SelectMany(r => r.Permissions)
            .Distinct();
    }
}
