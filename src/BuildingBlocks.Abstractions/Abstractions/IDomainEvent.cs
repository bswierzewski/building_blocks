using MediatR;

namespace BuildingBlocks.Kernel.Abstractions;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}