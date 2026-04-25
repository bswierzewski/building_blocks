using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.DeleteRole;

public sealed class DeleteRoleHandler
{
    [WolverineDelete("/api/identity/roles/{id}")]
    [Tags("Identity")]
    [EndpointName("DeleteRole")]
    [EndpointSummary("Delete role")]
    public static async Task Handle(
        Guid id,
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (role is null)
            throw new NotFoundException($"Role with ID {id} was not found");

        dbContext.Roles.Remove(role);
    }
}