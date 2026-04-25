using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.GetRole;

public sealed class GetRoleHandler
{
    [WolverineGet("/identity/roles/{id}")]
    [Tags("Identity")]
    [EndpointName("GetRole")]
    [EndpointSummary("Get role by ID")]
    public static async Task<GetRoleResponse> Handle(
        Guid id,
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var role = await dbContext.Roles
            .AsNoTracking()
            .Include(entity => entity.Users)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (role is null)
            throw new NotFoundException($"Role with ID {id} was not found");

        return new GetRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Permissions
                .OrderBy(permission => permission, StringComparer.Ordinal)
                .ToArray(),
            role.Users
                .Select(user => new GetRoleUserResponse(
                    user.Id,
                    user.Email.Value,
                    user.Status))
                .OrderBy(user => user.Email, StringComparer.Ordinal)
                .ToArray());
    }
}