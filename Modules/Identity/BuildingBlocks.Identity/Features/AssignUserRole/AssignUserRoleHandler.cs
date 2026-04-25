using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.AssignUserRole;

public sealed class AssignUserRoleHandler
{
    [WolverinePut("/api/identity/users/{id}/roles/{roleId}")]
    [Tags("Identity")]
    [EndpointName("AssignUserRole")]
    [EndpointSummary("Assign role to user")]
    public static async Task Handle(
        Guid id,
        Guid roleId,
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var user = await dbContext.Users
            .Include(entity => entity.Roles)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (user is null)
            throw new NotFoundException($"User with ID {id} was not found");

        var role = await dbContext.Roles
            .FirstOrDefaultAsync(entity => entity.Id == roleId, ct);

        if (role is null)
            throw new NotFoundException($"Role with ID {roleId} was not found");

        user.AssignRole(role);
    }
}