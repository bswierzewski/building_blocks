using BuildingBlocks.Domain.Primitives;

namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Represents a role that groups permissions together.
/// Roles can be assigned to users to grant them multiple permissions at once.
/// </summary>
public class Role : Entity<Guid>
{
    private readonly List<Permission> _permissions = new();

    /// <summary>
    /// Gets the unique name of the role (e.g., "Users.Admin", "Orders.Viewer").
    /// Format: {ModuleName}.{RoleName}
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the display name for UI purposes.
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Gets the optional description explaining what this role represents.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the name of the module this role belongs to.
    /// </summary>
    public string ModuleName { get; private set; } = null!;

    /// <summary>
    /// Gets the collection of permissions assigned to this role.
    /// </summary>
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { } // EF Core

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="name">Unique role name (e.g., "Users.Admin")</param>
    /// <param name="displayName">Display name for UI</param>
    /// <param name="moduleName">Module this role belongs to</param>
    /// <param name="description">Optional description</param>
    /// <returns>A new role instance</returns>
    public static Role Create(string name, string displayName, string moduleName, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Role display name cannot be empty", nameof(displayName));

        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name cannot be empty", nameof(moduleName));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            ModuleName = moduleName,
            Description = description
        };
    }

    /// <summary>
    /// Updates the role details.
    /// </summary>
    public void Update(string displayName, string? description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Role display name cannot be empty", nameof(displayName));

        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Adds a permission to this role.
    /// Idempotent - adding the same permission multiple times has no effect.
    /// </summary>
    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (!_permissions.Any(p => p.Id == permission.Id))
        {
            _permissions.Add(permission);
        }
    }

    /// <summary>
    /// Removes a permission from this role.
    /// </summary>
    public void RemovePermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        _permissions.RemoveAll(p => p.Id == permission.Id);
    }

    /// <summary>
    /// Removes a permission by its ID.
    /// </summary>
    public void RemovePermission(Guid permissionId)
    {
        _permissions.RemoveAll(p => p.Id == permissionId);
    }

    /// <summary>
    /// Checks if this role has a specific permission.
    /// </summary>
    public bool HasPermission(string permissionName)
    {
        return _permissions.Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this role has a specific permission by ID.
    /// </summary>
    public bool HasPermission(Guid permissionId)
    {
        return _permissions.Any(p => p.Id == permissionId);
    }
}
