using BuildingBlocks.Modules.Users.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserByExternalIdentity;

/// <summary>
/// Handler for retrieving a user by their external identity.
/// </summary>
public class GetUserByExternalIdentityQueryHandler : IRequestHandler<GetUserByExternalIdentityQuery, GetUserByExternalIdentityDto?>
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
    public async Task<GetUserByExternalIdentityDto?> Handle(GetUserByExternalIdentityQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .Include(u => u.ExternalIdentities)
            .FirstOrDefaultAsync(u =>
                u.ExternalIdentities.Any(e =>
                    e.Provider == request.Provider &&
                    e.ExternalUserId == request.ExternalUserId),
                cancellationToken);

        if (user == null)
            return null;

        return new GetUserByExternalIdentityDto
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            RoleIds = user.Roles.Select(r => r.Id).ToArray(),
            PermissionIds = user.GetAllPermissions().Select(p => p.Id).Distinct().ToArray()
        };
    }
}
