using BuildingBlocks.Domain.Primitives;

namespace BuildingBlocks.Domain.Entities;

/// <summary>
/// Represents a permission that can be assigned to roles.
/// Permissions define granular access rights within the system.
/// </summary>
public class Permission : Entity<Guid>
{
    /// <summary>
    /// Gets the unique name of the permission (e.g., "Users.Read", "Orders.Write").
    /// Format: {ModuleName}.{Resource}.{Action}
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the display name for UI purposes.
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Gets the optional description explaining what this permission allows.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the name of the module this permission belongs to.
    /// </summary>
    public string ModuleName { get; private set; } = null!;

    private Permission() { } // EF Core

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    /// <param name="name">Unique permission name (e.g., "Users.Read")</param>
    /// <param name="displayName">Display name for UI</param>
    /// <param name="moduleName">Module this permission belongs to</param>
    /// <param name="description">Optional description</param>
    /// <returns>A new permission instance</returns>
    public static Permission Create(string name, string displayName, string moduleName, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Permission display name cannot be empty", nameof(displayName));

        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name cannot be empty", nameof(moduleName));

        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            ModuleName = moduleName,
            Description = description
        };
    }

    /// <summary>
    /// Updates the permission details.
    /// </summary>
    public void Update(string displayName, string? description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Permission display name cannot be empty", nameof(displayName));

        DisplayName = displayName;
        Description = description;
    }
}
