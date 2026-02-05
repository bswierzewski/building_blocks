using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace BuildingBlocks.Infrastructure.Exceptions;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds diagnostic information to ProblemDetails responses, including TraceId, Timestamp, and Instance path.
    /// </summary>
    public static ProblemDetailsOptions AddDiagnosticInformation(this ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            // Add TraceId (uses Activity ID if exists, or HttpContext TraceIdentifier)
            context.ProblemDetails.Extensions.TryAdd(
                "traceId",
                Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

            // Add Timestamp
            context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);

            // Set instance to request path if not already set (ASP.NET usually does this)
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        };

        return options;
    }
}
