using BuildingBlocks.Abstractions.Abstractions;
using BuildingBlocks.Host.Queries.GetHealth;
using BuildingBlocks.Host.Queries.GetProtectedData;
using MediatR;

namespace BuildingBlocks.Host.Endpoints;

/// <summary>
/// Extension methods for mapping host management endpoints.
/// Provides HTTP endpoints for health checks and protected data access.
/// </summary>
public static class HostEndpoints
{
    /// <summary>
    /// Maps all HTTP endpoints for host operations.
    /// Establishes the routing structure for the Host API.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder used to configure API routes</param>
    /// <remarks>
    /// All endpoints are configured with OpenAPI support.
    /// </remarks>
    public static void MapHostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Public health check endpoint
        endpoints.MapGet("/health", GetHealth)
            .WithName("HealthCheck")
            .WithDescription("Get the health status of the application")
            .Produces<GetHealthResponse>(StatusCodes.Status200OK)
            .WithOpenApi();

        // Protected endpoint requiring authentication
        endpoints.MapGet("/protected", GetProtectedData)
            .WithName("GetProtectedData")
            .WithDescription("Get protected data for the authenticated user")
            .Produces<GetProtectedDataResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithOpenApi();
    }

    /// <summary>
    /// Retrieves the health status of the application.
    /// Returns current status and timestamp.
    /// </summary>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with health status</returns>
    private static async Task<IResult> GetHealth(IMediator mediator)
    {
        var query = new GetHealthQuery();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    /// <summary>
    /// Retrieves protected data for the currently authenticated user.
    /// Returns user information including roles.
    /// </summary>
    /// <param name="userContext">The current authenticated user context</param>
    /// <param name="mediator">MediatR instance for executing the query through the application layer</param>
    /// <returns>HTTP 200 OK with protected data</returns>
    /// <remarks>
    /// This endpoint requires authentication.
    /// The returned data includes user ID and assigned roles.
    /// </remarks>
    private static async Task<IResult> GetProtectedData(
        IUserContext userContext,
        IMediator mediator)
    {
        var userId = userContext.Email;

        var query = new GetProtectedDataQuery(userId);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }
}
