using BuildingBlocks.Core.Attributes;
using BuildingBlocks.Core.Authentication;
using BuildingBlocks.Core.Exceptions;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that enforces authorization for handlers decorated with AuthorizeAttribute.
/// </summary>
public static class AuthorizationMiddleware
{
    /// <summary>
    /// Executes before the handler and enforces authentication plus optional role checks.
    /// Only runs when AuthorizeAttribute is present on the handler type or method.
    /// </summary>
    public static void Before(AuthorizeAttribute authorize, ICurrentUser currentUser)
    {
        if (string.IsNullOrWhiteSpace(currentUser.Id))
            throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(authorize.Roles))
            return;

        var allowedRoles = authorize.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (allowedRoles.Any(currentUser.HasRole))
            return;

        throw new ForbiddenAccessException();
    }
}