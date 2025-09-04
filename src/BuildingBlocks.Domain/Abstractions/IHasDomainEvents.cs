namespace BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Interface for entities that can raise domain events.
/// Provides functionality to collect and manage domain events that should be published
/// when the entity is persisted or at other appropriate times.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the read-only collection of domain events
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Removes a specific domain event from the aggregate's event collection
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate's event collection
    /// </summary>
    void ClearDomainEvents();
}