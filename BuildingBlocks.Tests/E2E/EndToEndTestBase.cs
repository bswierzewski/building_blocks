using Aspire.Hosting.ApplicationModel;
using Xunit;

namespace BuildingBlocks.Tests.E2E;

/// <summary>
/// Base class for Aspire end-to-end tests.
/// A single EndToEndTestEnvironment should be shared through one xUnit collection.
/// The distributed application is started once per collection and reused by all tests in that collection.
/// </summary>
public abstract class EndToEndTestBase<TAppHost>(EndToEndTestEnvironment<TAppHost> testEnvironment)
    where TAppHost : class
{
    protected EndToEndTestEnvironment<TAppHost> TestEnvironment { get; } = testEnvironment;

    /// <summary>
    /// Creates an HTTPS client for the configured front-door resource or for the specified resource.
    /// </summary>
    protected HttpClient CreateHttpsClient(string? resourceName = null)
        => TestEnvironment.CreateHttpsClient(resourceName);

    /// <summary>
    /// Waits until the specified resource is reported as healthy by the Aspire test host.
    /// </summary>
    protected Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
        => TestEnvironment.WaitForResourceHealthyAsync(resourceName, cancellationToken);
}