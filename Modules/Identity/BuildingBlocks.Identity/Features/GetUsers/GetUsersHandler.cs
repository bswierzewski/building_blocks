using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.GetUsers;

public sealed class GetUsersHandler
{
    [WolverineGet("/identity/users")]
    [Tags("Identity")]
    [EndpointName("GetUsers")]
    [EndpointSummary("Get all users")]
    public static async Task<List<GetUsersResponse>> Handle(
        IdentityDbContext dbContext,
        CancellationToken ct)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.Roles)
            .Include(user => user.ExternalAccounts)
            .OrderBy(user => user.Email)
            .ToListAsync(ct);

        return users
            .Select(user => new GetUsersResponse(
                user.Id,
                user.Email.Value,
                user.Status,
                user.Roles
                    .Select(role => role.Name)
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToArray(),
                user.ExternalAccounts
                    .Select(account => new GetUsersExternalAccountResponse(
                        account.Id,
                        account.Provider,
                        account.ExternalId))
                    .OrderBy(account => account.Provider)
                    .ThenBy(account => account.ExternalId, StringComparer.Ordinal)
                    .ToArray()))
            .ToList();
    }
}