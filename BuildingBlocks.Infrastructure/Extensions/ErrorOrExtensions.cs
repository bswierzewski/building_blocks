using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Extensions;

/// <summary>
/// Maps ErrorOr results to RFC 7807-compatible minimal API responses.
/// </summary>
public static class ErrorOrExtensions
{
    // Fallback message used when no explicit error details are available.
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";
    private const string ValidationErrorDetail = "One or more validation failures have occurred.";

    /// <summary>
    /// Converts a list of errors into an HTTP problem response.
    /// </summary>
    public static IResult Problem(this List<Error> errors) => CreateProblemResult(errors);

    /// <summary>
    /// Converts a single error into an HTTP problem response.
    /// </summary>
    public static IResult Problem(this Error error) => CreateProblemResult([error]);

    // Keep all HTTP mapping rules in one place so response creation stays simple.
    private static IResult CreateProblemResult(List<Error> errors)
    {
        if (errors is null or { Count: 0 })
            return Results.Problem(UnexpectedErrorMessage);

        var firstError = errors[0];
        var metadata = GetProblemMetadata(firstError.Type);

        if (firstError.Type == ErrorType.Validation)
            return Results.ValidationProblem(
                errors.ToDictionary(),
                detail: ValidationErrorDetail,
                title: metadata.Title,
                type: metadata.Type,
                statusCode: metadata.StatusCode);

        return Results.Problem(
            detail: firstError.Description,
            statusCode: metadata.StatusCode,
            title: metadata.Title,
            type: metadata.Type);
    }

    private static (int StatusCode, string Title, string Type) GetProblemMetadata(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => (
            StatusCodes.Status400BadRequest,
            "Bad Request",
            "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
        ErrorType.NotFound => (
            StatusCodes.Status404NotFound,
            "Not Found",
            "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
        ErrorType.Unauthorized => (
            StatusCodes.Status401Unauthorized,
            "Unauthorized",
            "https://tools.ietf.org/html/rfc7235#section-3.1"),
        ErrorType.Forbidden => (
            StatusCodes.Status403Forbidden,
            "Forbidden",
            "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
        ErrorType.Conflict => (
            StatusCodes.Status409Conflict,
            "Conflict",
            "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
        ErrorType.Failure => (
            StatusCodes.Status400BadRequest,
            "Bad Request",
            "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
        ErrorType.Unexpected => (
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "https://tools.ietf.org/html/rfc7231#section-6.6.1"),
        _ => (
            StatusCodes.Status500InternalServerError,
            "An error occurred",
            "https://tools.ietf.org/html/rfc7231#section-6.6.1")
    };

    // Validation errors are grouped by code so ASP.NET can emit a standard validation payload.
    private static Dictionary<string, string[]> ToDictionary(this IEnumerable<Error> errors) =>
        errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());
}