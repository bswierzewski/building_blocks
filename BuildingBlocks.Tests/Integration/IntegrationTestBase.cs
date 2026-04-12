using Alba;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base class for integration tests.
/// For every test: resets the database, creates the Alba host, seeds data, and disposes after the test.
/// Collection-wide service overrides go in IntegrationTestCollection.ConfigureServices.
/// Per-class overrides go in ConfigureServices and SeedDataAsync here.
/// </summary>
public abstract class IntegrationTestBase<TProgram>(IntegrationTestCollection<TProgram> collection) : IAsyncLifetime
    where TProgram : class
{
    protected IAlbaHost AlbaHost { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await collection.ResetDatabaseAsync();

        AlbaHost = await Alba.AlbaHost.For<TProgram>(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = collection.ConnectionString
                }));

            builder.ConfigureServices((_, services) =>
            {
                services.AddSingleton(TimeProvider.System);
                collection.ConfigureServices(services);
                ConfigureServices(services);
            });
        });

        try
        {
            await SeedDataAsync();
        }
        catch
        {
            await AlbaHost.DisposeAsync();
            await collection.ResetDatabaseAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (AlbaHost is not null)
            await AlbaHost.DisposeAsync();
    }

    /// <summary>
    /// Override to configure service replacements used by all tests in this class.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Override to seed data after the host is created and before each test runs.
    /// </summary>
    protected virtual Task SeedDataAsync() => Task.CompletedTask;

    protected Task ResetDatabaseAsync() => collection.ResetDatabaseAsync();

    protected T GetRequiredService<T>() where T : notnull => AlbaHost.Services.GetRequiredService<T>();

    protected T? GetService<T>() where T : class => AlbaHost.Services.GetService<T>();
}
