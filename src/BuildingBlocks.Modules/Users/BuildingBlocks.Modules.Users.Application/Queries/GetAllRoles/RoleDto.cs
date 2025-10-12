namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllRoles;

/// <summary>
/// DTO representing a role with its permissions.
/// </summary>
public record RoleDto
{
    /// <summary>
    /// Gets the role ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique role name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; init; } = null!;

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the module this role belongs to.
    /// </summary>
    public string ModuleName { get; init; } = null!;

    /// <summary>
    /// Gets the list of permission names associated with this role.
    /// </summary>
    public string[] PermissionNames { get; init; } = Array.Empty<string>();
}
