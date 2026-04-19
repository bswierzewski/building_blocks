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
        if (string.IsNullOrWhiteSpace(currentUser.Id))
            throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(authorize.Permissions))
            return;

        var requiredPermissions = authorize.Permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (requiredPermissions.Any(currentUser.HasPermission))
            return;

        throw new ForbiddenAccessException();
    }
}