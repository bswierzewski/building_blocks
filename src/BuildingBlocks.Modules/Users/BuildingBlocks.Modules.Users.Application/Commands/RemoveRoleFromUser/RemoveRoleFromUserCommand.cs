using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Commands.RemoveRoleFromUser;

/// <summary>
/// Command to remove a role from a user.
/// </summary>
/// <param name="UserId">The ID of the user to remove the role from</param>
/// <param name="RoleId">The ID of the role to remove</param>
public record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : IRequest;
