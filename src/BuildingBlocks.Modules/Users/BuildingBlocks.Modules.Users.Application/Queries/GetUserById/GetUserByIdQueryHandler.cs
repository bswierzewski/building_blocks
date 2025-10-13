using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserById;

/// <summary>
/// Handler for retrieving a user by their internal ID.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdDto?>
{
    private readonly IUsersReadDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the GetUserByIdQueryHandler class.
    /// </summary>
    public GetUserByIdQueryHandler(IUsersReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Handles the query to retrieve a user by their internal ID.
    /// </summary>
    public async Task<GetUserByIdDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.CreateFrom(request.UserId);

        var user = await _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        return new GetUserByIdDto
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
