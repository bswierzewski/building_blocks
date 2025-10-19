using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Modules.Users.Infrastructure.Module;

/// <summary>
/// Users module implementation defining roles and permissions for user management.
/// </summary>
public class Module : IModule
{
    /// <inheritdoc />
    public string ModuleName => "Users";

    /// <inheritdoc />
    public string DisplayName => "User Management";

    /// <inheritdoc />
    public string? Description => "Manages users, roles, permissions, and authentication";

    /// <inheritdoc />
    public IEnumerable<Permission> GetPermissions()
    {
        // Empty implementation - permissions will be added when needed
        return [];
    }

    /// <inheritdoc />
    public IEnumerable<Role> GetRoles()
    {
        // Empty implementation - roles will be added when needed
        return [];
    }
}
