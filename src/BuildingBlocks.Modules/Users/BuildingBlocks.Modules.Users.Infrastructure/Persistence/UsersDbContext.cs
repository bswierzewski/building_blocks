using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.Entities;
using BuildingBlocks.Modules.Users.Infrastructure.Persistence.Configurations;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Database context for the Users module.
/// Implements both read and write context interfaces for CQRS pattern support.
/// </summary>
public class UsersDbContext : DbContext, IUsersWriteDbContext, IUsersReadDbContext
{
    /// <summary>
    /// Gets or sets the DbSet for users.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for roles.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for permissions.
    /// </summary>
    public DbSet<Permission> Permissions { get; set; } = null!;

    /// <summary>
    /// Provides read-only access to users with no tracking for performance optimization.
    /// </summary>
    IQueryable<User> IUsersReadDbContext.Users => Users.AsNoTracking();

    /// <summary>
    /// Provides read-only access to roles with no tracking for performance optimization.
    /// </summary>
    IQueryable<Role> IUsersReadDbContext.Roles => Roles.AsNoTracking();

    /// <summary>
    /// Provides read-only access to permissions with no tracking for performance optimization.
    /// </summary>
    IQueryable<Permission> IUsersReadDbContext.Permissions => Permissions.AsNoTracking();

    /// <summary>
    /// Initializes a new instance of the UsersDbContext.
    /// </summary>
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model and relationships for the Users module entities.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ExternalIdentityConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
