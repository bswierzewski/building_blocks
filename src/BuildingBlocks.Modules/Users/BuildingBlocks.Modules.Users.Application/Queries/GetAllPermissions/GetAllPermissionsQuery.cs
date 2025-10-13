using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllPermissions;

/// <summary>
/// Query to retrieve all permissions in the system.
/// Used by admin UI for role management.
/// </summary>
public record GetAllPermissionsQuery() : IRequest<IReadOnlyList<PermissionDto>>;

/// <summary>
/// DTO representing a permission.
/// </summary>
public record PermissionDto
{
    /// <summary>
    /// Gets the permission ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique permission name.
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
    /// Gets the module this permission belongs to.
    /// </summary>
    public string ModuleName { get; init; } = null!;
}
