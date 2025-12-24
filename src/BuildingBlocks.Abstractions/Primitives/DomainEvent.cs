using BuildingBlocks.Abstractions.Abstractions;

namespace BuildingBlocks.Abstractions.Primitives;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}