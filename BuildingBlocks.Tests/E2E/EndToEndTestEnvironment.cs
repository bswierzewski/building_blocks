using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.E2E;

/// <summary>
/// Shared runtime environment for one end-to-end test stack.
/// Starts the distributed application once per collection and exposes helper methods for tests.
/// </summary>
public abstract class EndToEndTestEnvironment<TAppHost> : IAsyncLifetime
    where TAppHost : class
{
    protected DistributedApplication App { get; private set; } = default!;

    /// <summary>
    /// Default resource exposed through the main HTTPS entrypoint used by end-to-end tests.
    /// Override this in project-specific environments when the application is exercised through
    /// a gateway or another single front door.
    /// </summary>
    protected virtual string DefaultHttpsResourceName
        => throw new InvalidOperationException("No default HTTPS resource configured for this end-to-end environment.");

    /// <summary>
    /// Builds and starts the distributed application for the current test collection.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        LoadEnvironment();

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<TAppHost>();
        ConfigureEnvironmentServices(builder.Services);

        App = await builder.BuildAsync();
        await App.StartAsync();
        await InitializeEnvironmentAsync();
    }

    /// <summary>
    /// Disposes the distributed application created for the current test collection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (App is not null)
            await App.DisposeAsync();
    }

    /// <summary>
    /// Creates an HTTPS client for the configured front-door resource or for the specified resource.
    /// </summary>
    public HttpClient CreateHttpsClient(string? resourceName = null)
        => App.CreateHttpClient(resourceName ?? DefaultHttpsResourceName, "https");

    /// <summary>
    /// Waits until the specified resource is reported as healthy by the Aspire test host.
    /// </summary>
    public Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
        => App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cancellationToken);

    /// <summary>
    /// Allows derived environments to register services used by the Aspire test host before the application is built.
    /// </summary>
    protected virtual void ConfigureEnvironmentServices(IServiceCollection services) { }

    /// <summary>
    /// Allows derived environments to load any required environment variables before the application is built.
    /// </summary>
    protected virtual void LoadEnvironment() { }

    /// <summary>
    /// Allows derived environments to perform additional readiness steps after the application has started.
    /// </summary>
    protected virtual ValueTask InitializeEnvironmentAsync()
        => ValueTask.CompletedTask;
}