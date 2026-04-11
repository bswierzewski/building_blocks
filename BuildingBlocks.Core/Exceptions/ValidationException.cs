namespace BuildingBlocks.Core.Exceptions;

/// <summary>
/// Exception thrown when custom validation fails in business logic.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : this(new Dictionary<string, string[]>()) { }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        ArgumentNullException.ThrowIfNull(errors);

        Errors = errors.ToDictionary(
            error => error.Key,
            error => error.Value.Distinct().ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}