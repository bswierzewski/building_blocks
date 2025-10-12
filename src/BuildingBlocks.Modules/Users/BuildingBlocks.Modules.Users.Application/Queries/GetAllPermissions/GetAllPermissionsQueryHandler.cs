using BuildingBlocks.Modules.Users.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetAllPermissions;

/// <summary>
/// Handler for retrieving all permissions.
/// </summary>
public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    private readonly IUsersReadDbContext _readContext;

    /// <summary>
    /// Initializes a new instance of the GetAllPermissionsQueryHandler class.
    /// </summary>
    public GetAllPermissionsQueryHandler(IUsersReadDbContext readContext)
    {
        _readContext = readContext;
    }

    /// <summary>
    /// Handles the query to retrieve all permissions.
    /// </summary>
    public async Task<IReadOnlyList<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _readContext.Permissions
            .OrderBy(p => p.ModuleName)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                ModuleName = p.ModuleName
            })
            .ToListAsync(cancellationToken);

        return permissions.AsReadOnly();
    }
}
