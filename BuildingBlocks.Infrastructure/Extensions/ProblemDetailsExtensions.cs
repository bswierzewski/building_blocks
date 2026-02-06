using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Infrastructure.Extensions;

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
            context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);

            // Add Timestamp
            context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);

            // Set instance to request path if not already set (ASP.NET usually does this)
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;

            // Add detail for validation errors
            if (context.ProblemDetails is ValidationProblemDetails validationProblem)
            {
                // If detail not already set
                if (string.IsNullOrEmpty(context.ProblemDetails.Detail))
                {
                    var totalErrors = validationProblem.Errors.Sum(e => e.Value.Length);
                    context.ProblemDetails.Detail = totalErrors == 1
                        ? "Request validation failed. See 'errors' property for details."
                        : $"Request validation failed with {totalErrors} error(s). See 'errors' property for details.";
                }
            }
        };

        return options;
    }
}
