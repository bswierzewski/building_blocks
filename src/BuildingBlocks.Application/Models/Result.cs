namespace BuildingBlocks.Application.Models;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Provides a simplified way to handle success and error cases.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The collection of error messages.</param>
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the array of error messages.
    /// </summary>
    public string[] Errors { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <returns>A failed result instance.</returns>
    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}