using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.GetRoles;

public sealed class GetRolesHandler
{
    [WolverineGet("/api/identity/roles")]
    [Tags("Identity")]
    [EndpointName("GetRoles")]
    [EndpointSummary("Get all roles")]
    public static async Task<List<GetRolesResponse>> Handle(
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.Users)
            .OrderBy(role => role.Name)
            .ToListAsync(ct);

        return roles
            .Select(role => new GetRolesResponse(
                role.Id,
                role.Name,
                role.Description,
                role.Permissions
                    .OrderBy(permission => permission, StringComparer.Ordinal)
                    .ToArray(),
                role.Users.Count))
            .ToList();
    }
}