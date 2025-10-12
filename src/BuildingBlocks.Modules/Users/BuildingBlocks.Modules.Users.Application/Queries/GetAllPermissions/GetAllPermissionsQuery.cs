using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllPermissions;

/// <summary>
/// Query to retrieve all permissions in the system.
/// Used by admin UI for role management.
/// </summary>
public record GetAllPermissionsQuery() : IRequest<IReadOnlyList<PermissionDto>>;
