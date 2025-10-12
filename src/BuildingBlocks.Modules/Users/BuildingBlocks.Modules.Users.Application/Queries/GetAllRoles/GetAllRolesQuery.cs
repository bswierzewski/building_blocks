using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllRoles;

/// <summary>
/// Query to retrieve all roles in the system.
/// Used by admin UI for user management.
/// </summary>
public record GetAllRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;
