using BuildingBlocks.Abstractions.Abstractions;

namespace BuildingBlocks.Abstractions.Primitives;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public Guid ModifiedBy { get; set; }
}