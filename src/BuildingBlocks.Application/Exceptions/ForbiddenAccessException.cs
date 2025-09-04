namespace BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exception thrown when access to a resource is forbidden.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ForbiddenAccessException class.
    /// </summary>
    public ForbiddenAccessException() : base() { }

    /// <summary>
    /// Initializes a new instance of the ForbiddenAccessException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ForbiddenAccessException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ForbiddenAccessException class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ForbiddenAccessException(string message, Exception innerException) : base(message, innerException) { }
}