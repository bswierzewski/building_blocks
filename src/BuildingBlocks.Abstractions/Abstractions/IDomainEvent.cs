using MediatR;

namespace BuildingBlocks.Abstractions.Abstractions;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}