using BuildingBlocks.Kernel.Abstractions;

namespace BuildingBlocks.Kernel.Primitives;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}