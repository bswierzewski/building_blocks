using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ErrorOrExtensions
{
    public static IResult Problem(this List<Error> errors)
    {
        if (errors is null or { Count: 0 })
            return Results.Problem("An unexpected error occurred.");

        var firstError = errors[0];
        var statusCode = GetStatusCodeForErrorType(firstError.Type);

        if (firstError.Type == ErrorType.Validation)
        {
            return Results.ValidationProblem(
                errors.ToDictionary(),
                detail: "One or more validation failures have occurred.",
                title: GetTitleForStatusCode(statusCode),
                type: GetTypeForStatusCode(statusCode),
                statusCode: statusCode);
        }

        return Results.Problem(
            detail: firstError.Description,
            statusCode: statusCode,
            title: GetTitleForStatusCode(statusCode),
            type: GetTypeForStatusCode(statusCode));
    }

    public static IResult Problem<TValue>(this ErrorOr<TValue> result)
    {
        if (result.IsError)
            return result.Errors.Problem();

        return Results.Problem("An unexpected error occurred.");
    }

    private static int GetStatusCodeForErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status400BadRequest,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "An error occurred"
    };

    private static string GetTypeForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static Dictionary<string, string[]> ToDictionary(this List<Error> errors)
    {
        return errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray()
            );
    }
}
