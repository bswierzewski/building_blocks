using BuildingBlocks.Kernel.Abstractions;
using BuildingBlocks.Kernel.Attributes;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BuildingBlocks.Infrastructure.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    IUserContext userContext,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        // If no authorization attributes found, continue without authorization checks
        if (!authorizeAttributes.Any())
            return await next(cancellationToken);

        // Check authorization requirements
        foreach (var attribute in authorizeAttributes)
        {
            foreach (var role in attribute.Roles)
            {
                if (!userContext.IsInRole(role))
                {
                    logger.LogWarning(
                        "Authorization failed for user {UserId} on request {RequestName}: User is not in the required role: {Role}",
                        userContext.Id,
                        typeof(TRequest).Name,
                        role);

                    return (dynamic)Error.Forbidden(
                        code: "Auth.Forbidden",
                        description: $"User is not in the required role: {role}");
                }
            }
        }

        // Authorization passed, continue to next behavior
        return await next(cancellationToken);
    }
}

