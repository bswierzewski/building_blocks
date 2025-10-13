using FluentValidation;
using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Commands.RemoveRoleFromUser;

/// <summary>
/// Validator for RemoveRoleFromUserCommand.
/// </summary>
public class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the RemoveRoleFromUserCommandValidator class.
    /// </summary>
    public RemoveRoleFromUserCommandValidator()
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
/// Command to remove a role from a user.
/// </summary>
/// <param name="UserId">The ID of the user to remove the role from</param>
/// <param name="RoleId">The ID of the role to remove</param>
public record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : IRequest;
