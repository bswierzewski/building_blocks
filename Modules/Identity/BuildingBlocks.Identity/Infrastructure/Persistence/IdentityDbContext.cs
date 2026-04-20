using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Identity.Domain.Entity;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Identity.Infrastructure.Persistence;

/// <summary>
/// Provides a design-time factory for creating instances of the application's Identity database context.
/// </summary>
public sealed class Factory : ModuleDbContextDesignTimeFactory<IdentityDbContext> { }

/// <summary>
/// EF Core DbContext for the Identity module.
/// </summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : ModuleDbContext<IdentityDbContext>(options, SchemaName)
{
    /// <summary>
    /// Database schema used by the Identity module tables and migration history.
    /// </summary>
    public const string SchemaName = "identity";

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<ExternalAccount> ExternalAccounts => Set<ExternalAccount>();
}