using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.E2E;

public abstract class EndToEndTestEnvironment : IAsyncLifetime
{
  public abstract ValueTask InitializeAsync();

  public abstract ValueTask DisposeAsync();
}

public abstract class EndToEndTestEnvironment<TAppHost> : EndToEndTestEnvironment
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

  protected virtual ValueTask ConfigureTestingServicesAsync(IServiceCollection services)
      => ValueTask.CompletedTask;

  protected virtual ValueTask OnApplicationStartedAsync()
      => ValueTask.CompletedTask;

  public override async ValueTask InitializeAsync()
  {
    var builder = await DistributedApplicationTestingBuilder.CreateAsync<TAppHost>();
    await ConfigureTestingServicesAsync(builder.Services);

    App = await builder.BuildAsync();
    await App.StartAsync();
    await OnApplicationStartedAsync();
  }

  public override async ValueTask DisposeAsync()
  {
    if (App is not null)
      await App.DisposeAsync();
  }

  public HttpClient CreateHttpsClient(string? resourceName = null)
  {
    return App.CreateHttpClient(resourceName ?? DefaultHttpsResourceName, "https");
  }

  public Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cancellationToken);
  }
}