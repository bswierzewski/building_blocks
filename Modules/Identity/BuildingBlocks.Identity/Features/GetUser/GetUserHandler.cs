using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.GetUser;

public sealed class GetUserHandler
{
    [WolverineGet("/identity/users/{id}")]
    [Tags("Identity")]
    [EndpointName("GetUser")]
    [EndpointSummary("Get user by ID")]
    public static async Task<GetUserResponse> Handle(
        Guid id,
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(entity => entity.Roles)
            .Include(entity => entity.ExternalAccounts)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (user is null)
            throw new NotFoundException($"User with ID {id} was not found");

        return new GetUserResponse(
            user.Id,
            user.Email.Value,
            user.Status,
            user.Roles
                .Select(role => new GetUserRoleResponse(
                    role.Id,
                    role.Name,
                    role.Description,
                    role.Permissions
                        .OrderBy(permission => permission, StringComparer.Ordinal)
                        .ToArray()))
                .OrderBy(role => role.Name, StringComparer.Ordinal)
                .ToArray(),
            user.ExternalAccounts
                .Select(account => new GetUserExternalAccountResponse(
                    account.Id,
                    account.Provider,
                    account.ExternalId))
                .OrderBy(account => account.Provider)
                .ThenBy(account => account.ExternalId, StringComparer.Ordinal)
                .ToArray());
    }
}