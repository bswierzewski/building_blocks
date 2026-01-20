namespace BuildingBlocks.Kernel.Abstractions;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
    DateTimeOffset? ModifiedAt { get; set; }
    Guid ModifiedBy { get; set; }
}