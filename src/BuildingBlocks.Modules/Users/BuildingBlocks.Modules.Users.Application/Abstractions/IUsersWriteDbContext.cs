using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Application.Abstractions;

/// <summary>
/// Write database context interface for User command operations.
/// Provides access to user data with change tracking.
/// </summary>
public interface IUsersWriteDbContext
{
    /// <summary>
    /// Gets write access to users with change tracking.
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Gets write access to roles with change tracking.
    /// </summary>
    DbSet<Role> Roles { get; }

    /// <summary>
    /// Gets write access to permissions with change tracking.
    /// </summary>
    DbSet<Permission> Permissions { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
