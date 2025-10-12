using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Aggregates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserByExternalIdentity;

/// <summary>
/// Handler for retrieving a user by their external identity.
/// </summary>
public class GetUserByExternalIdentityQueryHandler : IRequestHandler<GetUserByExternalIdentityQuery, User?>
{
    private readonly IUsersReadDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the GetUserByExternalIdentityQueryHandler class.
    /// </summary>
    public GetUserByExternalIdentityQueryHandler(IUsersReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the query to retrieve a user by their external identity.
    /// </summary>
    public async Task<User?> Handle(GetUserByExternalIdentityQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.ExternalIdentities.Any(e =>
                    e.Provider == request.Provider &&
                    e.ExternalUserId == request.ExternalUserId),
                cancellationToken);
    }
}
