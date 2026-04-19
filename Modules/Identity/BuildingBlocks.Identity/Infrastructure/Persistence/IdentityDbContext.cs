using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Identity.Domain.Entity;
using BuildingBlocks.Infrastructure.Persistence.DesignTime;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Identity.Infrastructure.Persistence;


/// <summary>
/// Design-time factory used by EF Core tools to create the Identity DbContext.
/// </summary>
public sealed class IdentityDbContextFactory : DesignTimeDbContextFactoryBase<IdentityDbContext> { }

/// <summary>
/// EF Core DbContext for the Identity module.
/// </summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<ExternalAccount> ExternalAccounts => Set<ExternalAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(typeof(IdentityDbContext).ToDbContextSchemaName());
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}