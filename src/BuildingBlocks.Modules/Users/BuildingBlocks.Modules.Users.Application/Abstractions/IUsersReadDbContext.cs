using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Modules.Users.Application.Abstractions;

/// <summary>
/// Read-only database context interface for User queries.
/// Provides access to user data with no-tracking for performance optimization.
/// </summary>
public interface IUsersReadDbContext
{
    /// <summary>
    /// Gets read-only access to users with no tracking.
    /// </summary>
    IQueryable<User> Users { get; }

    /// <summary>
    /// Gets read-only access to roles with no tracking.
    /// </summary>
    IQueryable<Role> Roles { get; }

    /// <summary>
    /// Gets read-only access to permissions with no tracking.
    /// </summary>
    IQueryable<Permission> Permissions { get; }
}
