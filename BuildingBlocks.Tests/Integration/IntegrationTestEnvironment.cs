using Alba;
using BuildingBlocks.Infrastructure.Modules;
using BuildingBlocks.Tests.Integration.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Shared runtime environment for one integration-test stack.
/// Manages the PostgreSQL container, schema initialization, and Respawn reset state.
/// </summary>
public abstract class IntegrationTestEnvironment<TProgram> : IAsyncLifetime
    where TProgram : class
{
    private readonly IntegrationTestDatabaseState _database = new();

    public string ConnectionString => _database.ConnectionString;

    /// <summary>
    /// Resets shared state, creates an Alba host for one test, and optionally runs per-test seed logic.
    /// </summary>
    public async Task<IAlbaHost> InitializeTestAsync(
        Action<IServiceCollection>? configureTestServices = null,
        Func<IAlbaHost, Task>? seedDataAsync = null)
    {
        await ResetDatabaseAsync();

        var host = await CreateHostAsync(configureTestServices);

        try
        {
            if (seedDataAsync is not null)
                await seedDataAsync(host);

            return host;
        }
        catch
        {
            await DisposeTestAsync(host);
            await ResetDatabaseAsync();
            throw;
        }
    }

    /// <summary>
    /// Creates the Alba host for one test using the shared environment configuration.
    /// </summary>
    public async Task<IAlbaHost> CreateHostAsync(Action<IServiceCollection>? configureTestServices = null)
    {
        LoadEnvironment();

        return await AlbaHost.For<TProgram>(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(ModuleExtensions.DefaultConnectionStringConfigurationKey, ConnectionString);
            ConfigureHost(builder);
            builder.ConfigureLogging(ConfigureLogging);

            builder.ConfigureServices((_, services) =>
            {
                services.AddSingleton(TimeProvider.System);
                ConfigureEnvironmentServices(services);
                configureTestServices?.Invoke(services);
            });
        });
    }

    /// <summary>
    /// Disposes the Alba host created for one integration test.
    /// </summary>
    public async ValueTask DisposeTestAsync(IAlbaHost? host)
    {
        if (host is not null)
            await host.DisposeAsync();
    }

    /// <summary>
    /// Allows a concrete environment to load any required environment variables before the test host is created.
    /// </summary>
    protected virtual void LoadEnvironment() { }

    /// <summary>
    /// Allows a concrete environment to tweak the test host builder before the Alba host is created.
    /// </summary>
    protected virtual void ConfigureHost(IWebHostBuilder builder) { }

    /// <summary>
    /// Allows a concrete environment to configure logging for all integration-test hosts.
    /// </summary>
    protected virtual void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    }

    /// <summary>
    /// Allows a concrete environment to register collection-wide services before the test host is built.
    /// </summary>
    protected virtual void ConfigureEnvironmentServices(IServiceCollection services) { }

    /// <summary>
    /// Starts the shared PostgreSQL container, initializes the schema, and prepares Respawn.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync(async _ => await InitializeEnvironmentAsync());
    }

    /// <summary>
    /// Resets the shared database state so each test starts from a clean baseline.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await _database.ResetAsync();
    }

    /// <summary>
    /// Disposes the shared PostgreSQL resources for the current test collection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    /// <summary>
    /// Allows a concrete environment to perform one-time startup after core resources are available
    /// and before the shared environment is marked ready for tests.
    /// </summary>
    protected virtual ValueTask InitializeEnvironmentAsync()
        => ValueTask.CompletedTask;
}