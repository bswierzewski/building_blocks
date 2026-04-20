using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Infrastructure.Exceptions.ProblemDetails;

/// <summary>
/// Provides extensions for enriching ProblemDetails responses with diagnostic data.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds diagnostic information to ProblemDetails responses, including trace identifier, timestamp, and request path.
    /// </summary>
    public static ProblemDetailsOptions AddDiagnosticInformation(this ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;

            problem.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            problem.Extensions.TryAdd("timestamp", DateTimeOffset.UtcNow);

            problem.Instance ??= context.HttpContext.Request.Path;

            if (problem is ValidationProblemDetails validationProblem && string.IsNullOrEmpty(problem.Detail))
            {
                var totalErrors = validationProblem.Errors.Sum(error => error.Value.Length);

                problem.Detail = totalErrors == 1
                    ? "Request validation failed. See 'errors' property for details."
                    : $"Request validation failed with {totalErrors} error(s). See 'errors' property for details.";
            }
        };

        return options;
    }
}