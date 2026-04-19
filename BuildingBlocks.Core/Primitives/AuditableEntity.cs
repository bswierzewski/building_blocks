namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Provides audit trail information for entities that track creation and modification history.
/// </summary>
public interface IAuditable
{
    /// <summary>Gets or sets when this entity was created.</summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the user ID who created this entity.</summary>
    Guid CreatedBy { get; set; }

    /// <summary>Gets or sets when this entity was last modified.</summary>
    DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>Gets or sets the user ID who last modified this entity.</summary>
    Guid? ModifiedBy { get; set; }
}

/// <summary>
/// Base class for entities that track creation and modification audit information
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    /// <summary>When this entity was created (defaults to UTC now)</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>User ID who created this entity</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>When this entity was last modified</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>User ID who last modified this entity</summary>
    public Guid? ModifiedBy { get; set; }
}