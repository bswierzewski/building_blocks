using BuildingBlocks.Core.Attributes;
using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity;

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
    public static void Before(AuthorizeAttribute authorize, ICurrentUser currentUser, IRolePermissionRegistry rolePermissionRegistry)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        if (authorize.Roles.Length > 0 && !authorize.Roles.Any(role => rolePermissionRegistry.HasRole(currentUser.Roles, role)))
            throw new ForbiddenAccessException();

        if (authorize.Permissions.Length == 0)
            return;

        if (authorize.Permissions.Any(permission => rolePermissionRegistry.HasPermission(currentUser.Roles, permission)))
            return;

        throw new ForbiddenAccessException();
    }
}