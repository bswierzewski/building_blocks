using BuildingBlocks.Core.Exceptions;
using FluentValidation;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.Exceptions.Extensions;

/// <summary>
/// Provides extensions for enriching ProblemDetails responses with diagnostic data.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Maps known application exceptions to a unified validation-style problem payload.
    /// </summary>
    public static HttpValidationProblemDetails ToValidationProblemDetails(this Exception exception, IHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(env);

        return exception switch
        {
            BuildingBlocks.Core.Exceptions.ValidationException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation errors",
                ex.Message,
                ex.Errors),
            FluentValidation.ValidationException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Validation errors",
                ex.Message,
                ex.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray())),
            NotFoundException ex => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message),
            DomainException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                ex.Message),
            UnauthorizedAccessException => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message),
            ForbiddenAccessException => CreateProblemDetails(
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please contact support.")
        };
    }

    /// <summary>
    /// Adds diagnostic information to ProblemDetails responses, including trace identifier, timestamp, and request path.
    /// </summary>
    public static ProblemDetailsOptions AddDiagnosticInformation(this ProblemDetailsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            var timestamp = DateTimeOffset.UtcNow;

            problem.Extensions.TryAdd("traceId", traceId);
            problem.Extensions.TryAdd("timestamp", timestamp);

            problem.Instance ??= context.HttpContext.Request.Path;

            if (problem is HttpValidationProblemDetails validationProblem && string.IsNullOrEmpty(problem.Detail))
            {
                var totalErrors = validationProblem.Errors.Sum(error => error.Value.Length);

                problem.Detail = totalErrors == 1
                    ? "Request validation failed. See 'errors' property for details."
                    : $"Request validation failed with {totalErrors} error(s). See 'errors' property for details.";
            }
        };

        return options;
    }

    private static HttpValidationProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        return new HttpValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}