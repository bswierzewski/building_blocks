using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Exceptions;
using BuildingBlocks.Application.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior that handles authorization for MediatR requests.
/// Checks roles, permissions, and claims from JWT tokens to authorize requests.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// <para>
/// This behavior checks authorization attributes on MediatR request classes:
/// <list type="bullet">
/// <item><description><b>Roles</b>: User must have at least one of the specified roles (OR logic)</description></item>
/// <item><description><b>Permissions</b>: User must have all specified permissions (AND logic)</description></item>
/// <item><description><b>Claims</b>: User must have all specified claims (AND logic)</description></item>
/// </list>
/// </para>
/// <para>
/// If user is not authenticated, throws <see cref="UnauthorizedAccessException"/>.
/// If user is authenticated but lacks required authorization, throws <see cref="ForbiddenAccessException"/>.
/// </para>
/// </remarks>
/// <param name="user">The current user service.</param>
/// <param name="logger">The logger instance.</param>
public class AuthorizationBehavior<TRequest, TResponse>(IUser user, ILogger<AuthorizationBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user = user;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// Handles the request with authorization checks.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next behavior or an authorization failure result.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        // If no authorization attributes found, continue without authorization checks
        if (!authorizeAttributes.Any())
        {
            return await next();
        }

        // Check if user is authenticated
        if (!_user.IsAuthenticated)
        {
            _logger.LogWarning(
                "Unauthorized access attempt to {RequestName} by unauthenticated user",
                typeof(TRequest).Name);

            throw new UnauthorizedAccessException("Authentication is required to access this resource.");
        }

        // Check authorization requirements
        foreach (var attribute in authorizeAttributes)
        {
            // Check required roles (OR logic - user needs at least one)
            if (!string.IsNullOrEmpty(attribute.Roles))
            {
                var requiredRoles = attribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim());

                var hasRequiredRole = requiredRoles.Any(role => _user.IsInRole(role));

                if (!hasRequiredRole)
                {
                    _logger.LogWarning(
                        "Authorization failed for user {UserId} on request {RequestName}: User does not have any of the required roles: {Roles}",
                        _user.Id?.ToString() ?? "Unknown",
                        typeof(TRequest).Name,
                        attribute.Roles);

                    throw new ForbiddenAccessException($"User does not have any of the required roles: {attribute.Roles}");
                }
            }

            // Check required permissions (AND logic - user needs all)
            if (!string.IsNullOrEmpty(attribute.Permissions))
            {
                var requiredPermissions = attribute.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim());

                foreach (var permission in requiredPermissions)
                {
                    if (!_user.HasPermission(permission))
                    {
                        _logger.LogWarning(
                            "Authorization failed for user {UserId} on request {RequestName}: User does not have the required permission: {Permission}",
                            _user.Id?.ToString() ?? "Unknown",
                            typeof(TRequest).Name,
                            permission);

                        throw new ForbiddenAccessException($"User does not have the required permission: {permission}");
                    }
                }
            }

            // Check required claims (AND logic - user needs all)
            if (!string.IsNullOrEmpty(attribute.Claims))
            {
                var requiredClaims = attribute.Claims.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim());

                foreach (var claim in requiredClaims)
                {
                    // Support claim format: "claimType" or "claimType:claimValue"
                    var claimParts = claim.Split(':', 2);
                    var claimType = claimParts[0];
                    var claimValue = claimParts.Length > 1 ? claimParts[1] : null;

                    if (!_user.HasClaim(claimType, claimValue))
                    {
                        _logger.LogWarning(
                            "Authorization failed for user {UserId} on request {RequestName}: User does not have the required claim: {Claim}",
                            _user.Id?.ToString() ?? "Unknown",
                            typeof(TRequest).Name,
                            claim);

                        throw new ForbiddenAccessException($"User does not have the required claim: {claim}");
                    }
                }
            }

            // Check custom policy (if implemented in the future)
            if (!string.IsNullOrEmpty(attribute.Policy))
            {
                // This could be extended to support custom policy evaluation
                // For now, we'll log a warning that policies are not yet supported
                _logger.LogWarning(
                    "Policy-based authorization '{Policy}' is not yet implemented",
                    attribute.Policy);
            }
        }

        // Authorization passed, continue to next behavior
        return await next();
    }
}

