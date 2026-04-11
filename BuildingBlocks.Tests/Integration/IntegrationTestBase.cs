using Alba;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base class for integration tests.
/// A single IntegrationTestEnvironment should be shared through one xUnit collection.
/// The test host is created, seeded, and cleaned automatically for each test.
/// </summary>
public abstract class IntegrationTestBase<TProgram>(IntegrationTestEnvironment<TProgram> testEnvironment) : IAsyncLifetime
    where TProgram : class
{
    protected IntegrationTestEnvironment<TProgram> TestEnvironment { get; } = testEnvironment;

    protected IAlbaHost AlbaHost { get; private set; } = default!;

    protected Task ResetDatabaseAsync()
    {
        return TestEnvironment.ResetDatabaseAsync();
    }

    /// <summary>
    /// Override to configure service replacements used by all tests in this class.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// Override to seed data after the host is created and before each test runs.
    /// </summary>
    protected virtual Task SeedDataAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await TestEnvironment.ResetDatabaseAsync();

        AlbaHost = await TestEnvironment.CreateHostAsync(ConfigureServices);

        try
        {
            await SeedDataAsync();
        }
        catch
        {
            await AlbaHost.DisposeAsync();
            await TestEnvironment.ResetDatabaseAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (AlbaHost is not null)
        {
            await AlbaHost.DisposeAsync();
        }

        await TestEnvironment.ResetDatabaseAsync();
    }

    /// <summary>
    /// Gets a required service from the current test host.
    /// </summary>
    protected T GetRequiredService<T>() where T : notnull
    {
        return AlbaHost.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a service from the current test host, or null if not found.
    /// </summary>
    protected T? GetService<T>() where T : class
    {
        return AlbaHost.Services.GetService<T>();
    }
}