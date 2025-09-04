using BuildingBlocks.Domain.Abstractions;

namespace BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for auditable entities that tracks creation and modification metadata.
/// This class extends the base entity functionality with audit trail capabilities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditableEntity{TId}"/> class.
/// </remarks>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// Can be null if the entity has never been modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// Can be null if the entity has never been modified.
    /// </summary>
    public string? ModifiedBy { get; set; }
}