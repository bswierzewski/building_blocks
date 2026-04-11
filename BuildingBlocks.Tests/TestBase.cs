using Alba;
using BuildingBlocks.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Xunit;

namespace BuildingBlocks.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides Alba host setup and database cleanup.
/// Uses a shared PostgreSQL container from DatabaseFixture and Respawn for database reset between tests.
/// </summary>
/// <typeparam name="TProgram">The Program class of the web application to test.</typeparam>
public abstract class TestBase<TProgram>(DatabaseFixture databaseFixture) : IAsyncLifetime
    where TProgram : class
{
    private Respawner _respawner = default!;
    private NpgsqlConnection _connection = default!;

    protected IAlbaHost AlbaHost { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        // Set connection string via environment variable BEFORE Program.cs executes
        // This is the only way to override configuration in minimal hosting
        // because ConfigureAppConfiguration runs AFTER Program.cs reads builder.Configuration
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", databaseFixture.ConnectionString);

        // Create Alba host with Program.cs configuration
        AlbaHost = await Alba.AlbaHost.For<TProgram>(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            builder.ConfigureServices((context, services) =>
            {
                // Add TimeProvider for tests
                services.AddSingleton(TimeProvider.System);

                // Allow test-specific service overrides
                ConfigureServices(services);
            });
        });

        // Initialize Respawner after migrations (which run in Program.cs)
        _connection = new NpgsqlConnection(databaseFixture.ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")],
            WithReseed = true
        });

        // Seed test data
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        await AlbaHost.DisposeAsync();
        await ResetDatabaseAsync();
        await _connection.DisposeAsync();
    }

    /// <summary>
    /// Resets the database to a clean state using Respawn.
    /// Can be called manually in tests if needed between operations.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    /// <summary>
    /// Override to configure additional services or replace services with mocks.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override in derived classes to replace services with mocks
    }

    /// <summary>
    /// Override to seed test data before each test runs.
    /// </summary>
    protected virtual Task SeedDataAsync()
    {
        // Override in derived classes to seed test data
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a required service from the Alba host's service provider.
    /// </summary>
    protected T GetRequiredService<T>() where T : notnull
    {
        return AlbaHost.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a service from the Alba host's service provider, or null if not found.
    /// </summary>
    protected T? GetService<T>() where T : class
    {
        return AlbaHost.Services.GetService<T>();
    }
}
