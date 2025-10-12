using BuildingBlocks.Modules.Users.Application.Commands.AssignRoleToUser;
using BuildingBlocks.Modules.Users.Application.Commands.RemoveRoleFromUser;
using BuildingBlocks.Modules.Users.Application.Queries.GetAllPermissions;
using BuildingBlocks.Modules.Users.Application.Queries.GetAllRoles;
using BuildingBlocks.Modules.Users.Application.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Modules.Users.Web.Endpoints;

/// <summary>
/// Provides HTTP endpoints for user management operations within the Users module.
/// This class implements the REST API layer for user-related operations including
/// role management, permission queries, and current user information retrieval.
/// All endpoints follow the CQRS pattern using MediatR for command and query handling.
/// </summary>
/// <remarks>
/// The Users module handles authentication, authorization, and user management with support
/// for multiple identity providers (Clerk, Auth0). These endpoints expose operations for
/// front-end applications and user administration interfaces.
/// </remarks>
public static class UsersEndpoints
{
    /// <summary>
    /// Configures and maps all HTTP endpoints for user management operations.
    /// This method establishes the routing structure for the Users API, grouping all user-related
    /// endpoints under the "/api/users" base path with appropriate OpenAPI documentation.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to configure API routes</param>
    /// <returns>The configured endpoint route builder for method chaining</returns>
    /// <remarks>
    /// All endpoints are configured with OpenAPI support for automatic API documentation generation.
    /// The "Users" tag groups these endpoints in the Swagger UI for better organization.
    /// Authentication is required for all endpoints - unauthorized requests will return 401.
    /// </remarks>
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Create a route group for all user endpoints with consistent base path and tagging
        // This approach ensures API consistency and simplifies routing management
        var group = endpoints.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(); // All user endpoints require authentication

        // GET /api/users/me - Query to get current authenticated user information
        // Returns user profile, roles, and permissions for UI personalization
        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithOpenApi()
            .WithDescription("Get current authenticated user with roles and permissions");

        // GET /api/users/roles - Query to get all available roles in the system
        // Used for administration interfaces and role assignment UIs
        group.MapGet("/roles", GetAllRoles)
            .WithName("GetAllRoles")
            .WithOpenApi()
            .WithDescription("Get all system roles with their permissions");

        // GET /api/users/permissions - Query to get all available permissions by module
        // Used for administration interfaces to understand system capabilities
        group.MapGet("/permissions", GetAllPermissions)
            .WithName("GetAllPermissions")
            .WithOpenApi()
            .WithDescription("Get all system permissions grouped by module");

        // POST /api/users/{userId}/roles/{roleId} - Command to assign a role to a user
        // Requires appropriate permissions for user management
        group.MapPost("/{userId}/roles/{roleId}", AssignRoleToUser)
            .WithName("AssignRoleToUser")
            .WithOpenApi()
            .WithDescription("Assign a role to a user (requires user management permissions)");

        // DELETE /api/users/{userId}/roles/{roleId} - Command to remove a role from a user
        // Requires appropriate permissions for user management
        group.MapDelete("/{userId}/roles/{roleId}", RemoveRoleFromUser)
            .WithName("RemoveRoleFromUser")
            .WithOpenApi()
            .WithDescription("Remove a role from a user (requires user management permissions)");

        return endpoints;
    }

    /// <summary>
    /// Retrieves the current authenticated user's information including roles and permissions.
    /// This endpoint is essential for client-side personalization and authorization decisions.
    /// </summary>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with current user details, roles, and permissions</returns>
    /// <remarks>
    /// This endpoint uses the IUser service which extracts claims from the JWT token.
    /// The returned data includes user profile, assigned roles, and computed permissions.
    /// Client applications typically call this on startup to initialize user context.
    /// </remarks>
    private static async Task<IResult> GetCurrentUser(IMediator mediator)
    {
        // Query the current user through application layer
        // IUser service handles JWT claim extraction automatically
        var query = new GetCurrentUserQuery();
        var result = await mediator.Send(query);

        // Return 200 OK with user data - includes profile, roles, and permissions
        // Client can use this for UI personalization and client-side authorization
        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves all system roles with their associated permissions.
    /// This endpoint supports role management interfaces and administrative operations.
    /// </summary>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with collection of all roles and their permissions</returns>
    /// <remarks>
    /// Roles are defined by modules implementing IModule interface.
    /// Each role contains a set of permissions that define what actions users can perform.
    /// This data is useful for dropdown lists in user management UIs.
    /// </remarks>
    private static async Task<IResult> GetAllRoles(IMediator mediator)
    {
        // Query all roles from the database
        // Roles are seeded at startup by RolesAndPermissionsHostedService
        var query = new GetAllRolesQuery();
        var result = await mediator.Send(query);

        // Return 200 OK with roles collection
        // Empty collection is valid if no roles have been defined yet
        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves all system permissions grouped by module.
    /// This endpoint supports administrative interfaces and helps understand system capabilities.
    /// </summary>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with collection of all permissions organized by module</returns>
    /// <remarks>
    /// Permissions are defined by modules implementing IModule interface.
    /// They are automatically discovered and seeded at application startup.
    /// The module grouping helps organize permissions logically for administration UIs.
    /// </remarks>
    private static async Task<IResult> GetAllPermissions(IMediator mediator)
    {
        // Query all permissions from the database
        // Permissions are seeded at startup by RolesAndPermissionsHostedService
        var query = new GetAllPermissionsQuery();
        var result = await mediator.Send(query);

        // Return 200 OK with permissions collection grouped by module
        // This helps administrative UIs organize permissions logically
        return Results.Ok(result);
    }

    /// <summary>
    /// Assigns a specific role to a user, granting them all permissions associated with that role.
    /// This endpoint requires appropriate user management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the role will be assigned</param>
    /// <param name="roleId">The unique identifier of the role to assign</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when role is successfully assigned</returns>
    /// <remarks>
    /// Role assignment is idempotent - assigning an already assigned role will not cause errors.
    /// Changes take effect on the user's next authentication (new JWT token).
    /// This operation is audited for security compliance.
    /// Authorization is handled by AuthorizationBehavior using [Authorize] attribute on the command.
    /// </remarks>
    private static async Task<IResult> AssignRoleToUser(
        Guid userId,
        Guid roleId,
        IMediator mediator)
    {
        // Execute the assign role command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new AssignRoleToUserCommand(userId, roleId);
        await mediator.Send(command);

        // Return 200 OK on success
        // User will receive new permissions on next login (token refresh)
        return Results.Ok(new { Success = true, Message = "Role assigned successfully" });
    }

    /// <summary>
    /// Removes a specific role from a user, revoking all permissions associated with that role.
    /// This endpoint requires appropriate user management permissions to execute.
    /// </summary>
    /// <param name="userId">The unique identifier of the user from whom the role will be removed</param>
    /// <param name="roleId">The unique identifier of the role to remove</param>
    /// <param name="mediator">MediatR instance for executing the command through the application layer</param>
    /// <returns>HTTP 200 OK when role is successfully removed</returns>
    /// <remarks>
    /// Role removal is idempotent - removing an already removed role will not cause errors.
    /// Changes take effect on the user's next authentication (new JWT token).
    /// This operation is audited for security compliance.
    /// Authorization is handled by AuthorizationBehavior using [Authorize] attribute on the command.
    /// </remarks>
    private static async Task<IResult> RemoveRoleFromUser(
        Guid userId,
        Guid roleId,
        IMediator mediator)
    {
        // Execute the remove role command through the application layer
        // Authorization checks are performed by AuthorizationBehavior
        var command = new RemoveRoleFromUserCommand(userId, roleId);
        await mediator.Send(command);

        // Return 200 OK on success
        // User will lose permissions on next login (token refresh)
        return Results.Ok(new { Success = true, Message = "Role removed successfully" });
    }
}
