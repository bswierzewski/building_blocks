using Alba;
using System;
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
/// Environment-wide service overrides go in IntegrationTestEnvironment.ConfigureServices.
/// Per-class overrides go in ConfigureServices and SeedDataAsync here.
/// </summary>
public abstract class IntegrationTestBase<TProgram>(IntegrationTestEnvironment<TProgram> testEnvironment) : IAsyncLifetime
    where TProgram : class
{
    private const string DefaultConnectionStringEnvironmentVariable = "ConnectionStrings__Default";

    protected IAlbaHost AlbaHost { get; private set; } = default!;

    /// <summary>
    /// Resets shared state, creates the Alba host, and runs per-test seed logic.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await testEnvironment.ResetDatabaseAsync();

        Environment.SetEnvironmentVariable(DefaultConnectionStringEnvironmentVariable, testEnvironment.ConnectionString);

        AlbaHost = await Alba.AlbaHost.For<TProgram>(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            builder.ConfigureServices((_, services) =>
            {
                services.AddSingleton(TimeProvider.System);
                testEnvironment.ConfigureServices(services);
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
            await testEnvironment.ResetDatabaseAsync();
            throw;
        }
    }

    /// <summary>
    /// Disposes the Alba host created for the current test.
    /// </summary>
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
    /// Resets the shared database state for the current integration-test environment.
    /// </summary>
    protected Task ResetDatabaseAsync() => testEnvironment.ResetDatabaseAsync();

    /// <summary>
    /// Resolves a required service from the Alba host service provider.
    /// </summary>
    protected T GetRequiredService<T>() where T : notnull => AlbaHost.Services.GetRequiredService<T>();

    /// <summary>
    /// Resolves an optional service from the Alba host service provider.
    /// </summary>
    protected T? GetService<T>() where T : class => AlbaHost.Services.GetService<T>();

    /// <summary>
    /// Override to seed data after the host is created and before each test runs.
    /// </summary>
    protected virtual Task SeedDataAsync() => Task.CompletedTask;
}
