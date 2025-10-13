using FluentValidation;
using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Commands.AssignRoleToUser;

/// <summary>
/// Validator for AssignRoleToUserCommand.
/// </summary>
public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the AssignRoleToUserCommandValidator class.
    /// </summary>
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required");
    }
}

/// <summary>
/// Command to assign a role to a user.
/// </summary>
/// <param name="UserId">The ID of the user to assign the role to</param>
/// <param name="RoleId">The ID of the role to assign</param>
public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest;
