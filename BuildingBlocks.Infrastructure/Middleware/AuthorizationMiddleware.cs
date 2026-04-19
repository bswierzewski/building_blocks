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
    /// Executes before the handler and enforces authentication plus optional permission checks.
    /// Only runs when AuthorizeAttribute is present on the handler type or method.
    /// </summary>
    public static void Before(AuthorizeAttribute authorize, ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        if (authorize.Roles.Length > 0 && !authorize.Roles.Any(currentUser.HasRole))
            throw new ForbiddenAccessException();

        if (authorize.Permissions.Length == 0)
            return;

        if (authorize.Permissions.Any(currentUser.HasPermission))
            return;

        throw new ForbiddenAccessException();
    }
}