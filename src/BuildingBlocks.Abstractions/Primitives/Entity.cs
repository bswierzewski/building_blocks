namespace BuildingBlocks.Kernel.Primitives;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; init; } = default!;
}