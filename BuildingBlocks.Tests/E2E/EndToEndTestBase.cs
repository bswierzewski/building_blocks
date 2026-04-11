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

  protected HttpClient CreateHttpClient(string resourceName, string? endpointName = null)
  {
    return endpointName is null
        ? TestEnvironment.CreateHttpClient(resourceName)
        : TestEnvironment.CreateHttpClient(resourceName, endpointName);
  }

  protected Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return TestEnvironment.WaitForResourceHealthyAsync(resourceName, cancellationToken);
  }

  protected Task WaitForResourceAsync(
      string resourceName,
      string state,
      CancellationToken cancellationToken = default)
  {
    return TestEnvironment.WaitForResourceAsync(resourceName, state, cancellationToken);
  }

  protected Task<string?> GetConnectionStringAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return TestEnvironment.GetConnectionStringAsync(resourceName, cancellationToken);
  }
}