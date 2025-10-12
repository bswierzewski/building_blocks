using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Modules.Users.Infrastructure.Module;

/// <summary>
/// Users module implementation defining roles and permissions for user management.
/// </summary>
public class UsersModule : IModule
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
        yield return Permission.Create("Users.Read", "View Users", ModuleName, "Can view user list and details");
        yield return Permission.Create("Users.Write", "Edit Users", ModuleName, "Can create and update users");
        yield return Permission.Create("Users.Delete", "Delete Users", ModuleName, "Can delete users from the system");
        yield return Permission.Create("Users.Roles.Read", "View Roles", ModuleName, "Can view available roles");
        yield return Permission.Create("Users.Roles.Assign", "Assign Roles", ModuleName, "Can assign and remove roles from users");
        yield return Permission.Create("Users.Roles.Manage", "Manage Roles", ModuleName, "Can create, update, and delete roles");
        yield return Permission.Create("Users.Permissions.Read", "View Permissions", ModuleName, "Can view available permissions");
        yield return Permission.Create("Users.Permissions.Manage", "Manage Permissions", ModuleName, "Can create, update, and delete permissions");
    }

    /// <inheritdoc />
    public IEnumerable<Role> GetRoles()
    {
        var permissions = GetPermissions().ToList();

        // Super Admin - full access to user management
        var superAdmin = Role.Create("Users.SuperAdmin", "User Super Administrator", ModuleName, "Full access to all user management features");

        foreach (var permission in permissions)
        {
            superAdmin.AddPermission(permission);
        }
        yield return superAdmin;

        // Admin - manage users and assign roles
        var admin = Role.Create(
            "Users.Admin",
            "User Administrator",
            ModuleName,
            "Can manage users and assign roles");

        admin.AddPermission(permissions.First(p => p.Name == "Users.Read"));
        admin.AddPermission(permissions.First(p => p.Name == "Users.Write"));
        admin.AddPermission(permissions.First(p => p.Name == "Users.Delete"));
        admin.AddPermission(permissions.First(p => p.Name == "Users.Roles.Read"));
        admin.AddPermission(permissions.First(p => p.Name == "Users.Roles.Assign"));
        yield return admin;

        // Viewer - read-only access
        var viewer = Role.Create(
            "Users.Viewer",
            "User Viewer",
            ModuleName,
            "Read-only access to user information");

        viewer.AddPermission(permissions.First(p => p.Name == "Users.Read"));
        viewer.AddPermission(permissions.First(p => p.Name == "Users.Roles.Read"));
        viewer.AddPermission(permissions.First(p => p.Name == "Users.Permissions.Read"));
        yield return viewer;
    }
}
