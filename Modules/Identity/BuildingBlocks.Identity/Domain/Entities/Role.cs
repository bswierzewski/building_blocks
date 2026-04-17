using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Identity.Domain.Entities;

/// <summary>
/// Aggregate representing a role in the system.
/// A role is a named collection of permissions that can be assigned to users.
/// </summary>
public sealed class Role : AuditableEntity<Guid>, IAggregateRoot
{
    private readonly HashSet<Permission> permissions = [];

    private Role() { }

    /// <summary>Unique role name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Human-readable description of the role.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Set of permissions assigned to this role.</summary>
    public IReadOnlyCollection<Permission> Permissions => [.. permissions];

    /// <summary>Creates a new role with an optional initial set of permissions.</summary>
    public static Role Create(string name, string description, IEnumerable<Permission>? permissions = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim()
        };

        if (permissions is null)
            return role;

        foreach (var permission in permissions)
            role.AddPermission(permission);

        return role;
    }

    /// <summary>Updates the role name and description.</summary>
    public void Rename(string name, string description)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);

        Name = name.Trim();
        Description = description.Trim();
    }

    /// <summary>Adds a permission to the role. Ignores duplicates.</summary>
    public void AddPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);
        permissions.Add(permission);
    }

    /// <summary>Removes a permission from the role by code. Idempotent — no-op when the permission is not present.</summary>
    public void RemovePermission(string permissionCode)
    {
        ArgumentNullException.ThrowIfNull(permissionCode);

        var normalizedCode = permissionCode.Trim();
        var existingPermission = permissions.FirstOrDefault(permission =>
            string.Equals(permission.Code, normalizedCode, StringComparison.OrdinalIgnoreCase));

        if (existingPermission is null)
            return;

        permissions.Remove(existingPermission);
    }
}