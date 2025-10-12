namespace BuildingBlocks.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for User aggregate.
/// Represents the internal, unified user ID used throughout the application.
/// </summary>
public record UserId
{
    /// <summary>
    /// Gets the unique identifier value.
    /// </summary>
    public Guid Value { get; init; }

    private UserId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new unique user identifier.
    /// </summary>
    public static UserId Create() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a user identifier from an existing Guid value.
    /// </summary>
    public static UserId CreateFrom(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));

        return new UserId(value);
    }

    /// <summary>
    /// Parses a string representation of a Guid into a UserId.
    /// </summary>
    public static UserId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid UserId format: {value}", nameof(value));

        return CreateFrom(guid);
    }

    /// <summary>
    /// Returns the user ID as a string.
    /// </summary>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Implicit conversion from UserId to Guid for EF Core.
    /// </summary>
    public static implicit operator Guid(UserId userId) => userId.Value;
}
