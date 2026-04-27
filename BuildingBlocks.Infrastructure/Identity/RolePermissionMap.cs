using System.Collections.ObjectModel;
using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Base class for application-specific role to permission mappings.
/// </summary>
public abstract class RolePermissionMap
{
    private readonly Lazy<IReadOnlyDictionary<string, string[]>> rolePermissions;

    protected RolePermissionMap()
    {
        rolePermissions = new Lazy<IReadOnlyDictionary<string, string[]>>(BuildRolePermissions, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Gets the application roles exposed by the concrete implementation.
    /// </summary>
    protected abstract IEnumerable<Role> GetRoles();

    /// <summary>
    /// Gets all configured roles keyed by normalized role name with normalized permission codes.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> RolePermissions => rolePermissions.Value;

    /// <summary>
    /// Resolves effective permission codes for the provided roles.
    /// </summary>
    public IReadOnlySet<string> GetPermissions(IEnumerable<string> roleNames)
    {
        if (roleNames?.Any() != true)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(Role.NormalizeName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SelectMany(roleName => RolePermissions.TryGetValue(roleName, out var permissions) ? permissions : [])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private IReadOnlyDictionary<string, string[]> BuildRolePermissions()
    {
        var configuredRoles = GetRoles() ?? [];

        var configuredRolePermissions = configuredRoles
            .GroupBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(role => role.Permissions)
                    .Select(permission => permission.Code)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(permission => permission, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return new ReadOnlyDictionary<string, string[]>(configuredRolePermissions);
    }
}