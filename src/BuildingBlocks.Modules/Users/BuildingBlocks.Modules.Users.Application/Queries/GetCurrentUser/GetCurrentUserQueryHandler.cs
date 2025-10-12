using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetCurrentUser;

/// <summary>
/// Handler for retrieving the current authenticated user.
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    private readonly IUsersReadDbContext _readContext;
    private readonly IUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the GetCurrentUserQueryHandler class.
    /// </summary>
    public GetCurrentUserQueryHandler(IUsersReadDbContext readContext, IUser currentUser)
    {
        _readContext = readContext;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Handles the query to retrieve the current authenticated user.
    /// </summary>
    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Get user ID from claims (injected by OnTokenValidated)
        if (!Guid.TryParse(_currentUser.Id, out var userIdGuid))
            return null;

        var userId = UserId.CreateFrom(userIdGuid);

        // Load user with roles and permissions
        var user = await _readContext.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        // Map to DTO
        return new CurrentUserDto
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            PictureUrl = _currentUser.PictureUrl, // From JWT token
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Name).ToArray(),
            Permissions = user.GetAllPermissions().Select(p => p.Name).Distinct().ToArray()
        };
    }
}
