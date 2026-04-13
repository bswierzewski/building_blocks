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

  protected HttpClient CreateHttpsClient(string? resourceName = null)
  {
    return TestEnvironment.CreateHttpsClient(resourceName);
  }

  protected Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return TestEnvironment.WaitForResourceHealthyAsync(resourceName, cancellationToken);
  }
}