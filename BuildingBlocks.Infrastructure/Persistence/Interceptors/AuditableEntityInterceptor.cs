using BuildingBlocks.Kernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically manages audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy)
/// on IAuditable entities when saving changes.
/// </summary>
public sealed class AuditableEntityInterceptor(IUserContext userContext, TimeProvider dateTime) : SaveChangesInterceptor
{
    private readonly TimeProvider _dateTime = dateTime;

    /// <summary>Intercepts synchronous SaveChanges calls to update audit fields</summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>Intercepts asynchronous SaveChangesAsync calls to update audit fields</summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>Updates audit fields for all IAuditable entities that are being added or modified</summary>
    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        // Iterate through all tracked entities implementing IAuditable
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            // Only update if entity is new, modified, or has changed owned entities (value objects)
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow();

                // Set creation audit fields only for new entities
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userContext.Id;
                    entry.Entity.CreatedAt = utcNow;
                }

                // Always update modification audit fields
                entry.Entity.ModifiedBy = userContext.Id;
                entry.Entity.ModifiedAt = utcNow;
            }
        }
    }
}

/// <summary>
/// Extension methods for EntityEntry to check for changes in owned entities (value objects)
/// </summary>
public static class EntityEntryExtensions
{
    /// <summary>
    /// Determines if any owned entities (value objects) of this entity have been added or modified
    /// </summary>
    /// <param name="entry">The entity entry to check</param>
    /// <returns>True if any owned entities have changed, false otherwise</returns>
    public static bool HasChangedOwnedEntities(this EntityEntry entry)
    {
        // Check navigation properties to see if owned entities have been modified
        return entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified || r.TargetEntry.HasChangedOwnedEntities()));
    }
}