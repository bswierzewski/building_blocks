using BuildingBlocks.Modules.Users.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllRoles;

/// <summary>
/// Handler for retrieving all roles.
/// </summary>
public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IUsersReadDbContext _readContext;

    /// <summary>
    /// Initializes a new instance of the GetAllRolesQueryHandler class.
    /// </summary>
    public GetAllRolesQueryHandler(IUsersReadDbContext readContext)
    {
        _readContext = readContext;
    }

    /// <summary>
    /// Handles the query to retrieve all roles.
    /// </summary>
    public async Task<IReadOnlyList<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _readContext.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.ModuleName)
            .ThenBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                DisplayName = r.DisplayName,
                Description = r.Description,
                ModuleName = r.ModuleName,
                PermissionNames = r.Permissions.Select(p => p.Name).ToArray()
            })
            .ToListAsync(cancellationToken);

        return roles.AsReadOnly();
    }
}
