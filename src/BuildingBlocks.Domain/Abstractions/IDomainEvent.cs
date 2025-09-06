namespace BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something significant that happened in the domain.
/// They are used to decouple different parts of the domain and enable side effects.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}