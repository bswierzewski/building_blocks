using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Commands.AssignRoleToUser;

/// <summary>
/// Handler for assigning a role to a user.
/// </summary>
public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IUsersWriteDbContext _writeContext;

    /// <summary>
    /// Initializes a new instance of the AssignRoleToUserCommandHandler class.
    /// </summary>
    public AssignRoleToUserCommandHandler(IUsersWriteDbContext writeContext)
    {
        _writeContext = writeContext;
    }

    /// <summary>
    /// Handles the command to assign a role to a user.
    /// </summary>
    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.CreateFrom(request.UserId);

        // Load user with roles
        var user = await _writeContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Load role
        var role = await _writeContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            throw new InvalidOperationException($"Role with ID {request.RoleId} not found");

        // Assign role
        user.AssignRole(role);

        await _writeContext.SaveChangesAsync(cancellationToken);
    }
}
