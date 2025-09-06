namespace BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marker interface for aggregate root entities in Domain-Driven Design.
/// Aggregate roots are the only entities that can be referenced from outside the aggregate boundary.
/// </summary>
public interface IAggregateRoot
{
}