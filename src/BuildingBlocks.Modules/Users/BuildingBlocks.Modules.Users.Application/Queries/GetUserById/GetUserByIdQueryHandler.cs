using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserById;

/// <summary>
/// Handler for retrieving a user by their internal ID.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
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
    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.CreateFrom(request.UserId);

        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}
