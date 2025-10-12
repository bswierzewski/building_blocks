using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Defines a module within the application.
/// Each module should implement this interface to register its roles and permissions.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the unique module name (e.g., "Users", "Orders", "Catalog").
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Gets the module display name for UI purposes.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the optional module description.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Defines all permissions for this module.
    /// Called during application startup to register permissions in the database.
    /// </summary>
    /// <returns>Collection of permissions for this module</returns>
    IEnumerable<Permission> GetPermissions();

    /// <summary>
    /// Defines all roles for this module with their associated permissions.
    /// Called during application startup to register roles in the database.
    /// </summary>
    /// <returns>Collection of roles for this module</returns>
    IEnumerable<Role> GetRoles();
}
