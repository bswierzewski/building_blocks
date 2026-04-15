using Alba;
using Microsoft.Extensions.DependencyInjection;
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
    protected IntegrationTestEnvironment<TProgram> TestEnvironment { get; } = testEnvironment;

    protected IAlbaHost AlbaHost { get; private set; } = default!;

    /// <summary>
    /// Delegates per-test host creation and seeding to the shared integration-test environment.
    /// </summary>
    public async ValueTask InitializeAsync()
        => AlbaHost = await TestEnvironment.InitializeTestAsync(ConfigureServices, _ => SeedDataAsync());

    /// <summary>
    /// Disposes the Alba host created for the current test.
    /// </summary>
    public ValueTask DisposeAsync() => TestEnvironment.DisposeTestAsync(AlbaHost);

    /// <summary>
    /// Override to configure service replacements used by all tests in this class.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Resets the shared database state for the current integration-test environment.
    /// </summary>
    protected Task ResetDatabaseAsync() => TestEnvironment.ResetDatabaseAsync();

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