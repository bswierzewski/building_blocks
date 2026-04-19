namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Represents a domain event that occurred in the system.
/// </summary>
public interface IEvent
{
    /// <summary>Gets the unique identifier of this event.</summary>
    Guid Id { get; }

    /// <summary>Gets the timestamp when this event occurred.</summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base record for domain events with automatic ID and timestamp generation
/// Supports both record and class syntax for derived events
/// </summary>
public abstract record Event : IEvent
{
    /// <summary>Unique identifier automatically generated when event is created</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Timestamp set to UTC now when event is created</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}