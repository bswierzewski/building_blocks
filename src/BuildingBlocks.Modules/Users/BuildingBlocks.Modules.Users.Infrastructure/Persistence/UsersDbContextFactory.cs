using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for UsersDbContext to support EF Core migrations.
/// This is used only during migration creation and not at runtime.
/// </summary>
public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    /// <summary>
    /// Creates a new instance of UsersDbContext for design-time operations.
    /// </summary>
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        // Minimal placeholder connection string for design-time migrations only
        // EF Core only needs to know the provider type (PostgreSQL) to generate migrations
        // No actual database connection is established during migration creation
        // The actual connection string is provided at runtime via dependency injection
        optionsBuilder.UseNpgsql("Host=localhost");

        return new UsersDbContext(optionsBuilder.Options);
    }
}
