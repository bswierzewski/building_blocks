using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Kernel.Abstractions;
using BuildingBlocks.Kernel.Attributes;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that enforces role-based authorization on message handlers.
/// Automatically applied to handlers decorated with [Authorize] attribute.
/// </summary>
public static class AuthorizationMiddleware
{
    /// <summary>
    /// Validates user has required roles before executing the handler.
    /// Wolverine automatically detects this method and generates optimized middleware code.
    /// </summary>
    public static void Before(
        AuthorizeAttribute authorizeAttribute,
        IUserContext userContext,
        ILogger logger,
        IMessageContext messageContext)
    {
        var messageType = messageContext.Envelope?.Message?.GetType().Name ?? "Unknown";
        
        // Check if user has any of the required roles
        foreach (var role in authorizeAttribute.Roles)
        {
            if (!userContext.IsInRole(role))
            {
                logger.LogWarning(
                    "Authorization failed for user {UserId} on message {MessageType}: User is not in the required role: {Role}",
                    userContext.Id,
                    messageType,
                    role);

                throw new ForbiddenAccessException($"User is not in the required role: {role}");
            }
        }
        
        // If no exception thrown, authorization passed
        logger.LogDebug(
            "Authorization successful for user {UserId} on message {MessageType}",
            userContext.Id,
            messageType);
    }
}
