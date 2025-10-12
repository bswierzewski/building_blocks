using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Commands.RemoveRoleFromUser;

/// <summary>
/// Handler for removing a role from a user.
/// </summary>
public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IUsersWriteDbContext _writeContext;

    /// <summary>
    /// Initializes a new instance of the RemoveRoleFromUserCommandHandler class.
    /// </summary>
    public RemoveRoleFromUserCommandHandler(IUsersWriteDbContext writeContext)
    {
        _writeContext = writeContext;
    }

    /// <summary>
    /// Handles the command to remove a role from a user.
    /// </summary>
    public async Task Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.CreateFrom(request.UserId);

        // Load user with roles
        var user = await _writeContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Remove role by ID
        user.RemoveRole(request.RoleId);

        await _writeContext.SaveChangesAsync(cancellationToken);
    }
}
