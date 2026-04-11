using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.E2E;

public abstract class EndToEndTestEnvironment : IAsyncLifetime
{
  public abstract Task InitializeAsync();

  public abstract Task DisposeAsync();
}

public abstract class EndToEndTestEnvironment<TAppHost> : EndToEndTestEnvironment
    where TAppHost : class
{
  protected static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(30);

  protected DistributedApplication App { get; private set; } = default!;

  protected virtual string[] GetAppHostArguments()
  {
    return [];
  }

  protected virtual Task ConfigureTestingServicesAsync(IServiceCollection services)
  {
    return Task.CompletedTask;
  }

  protected virtual Task OnApplicationStartedAsync()
  {
    return Task.CompletedTask;
  }

  public override async Task InitializeAsync()
  {
    var builder = await DistributedApplicationTestingBuilder.CreateAsync<TAppHost>(GetAppHostArguments());
    await ConfigureTestingServicesAsync(builder.Services);

    App = await builder.BuildAsync();
    await App.StartAsync();
    await OnApplicationStartedAsync();
  }

  public override async Task DisposeAsync()
  {
    if (App is not null)
    {
      await App.DisposeAsync();
    }
  }

  public HttpClient CreateHttpClient(string resourceName, string? endpointName = null)
  {
    return endpointName is null
        ? App.CreateHttpClient(resourceName)
        : App.CreateHttpClient(resourceName, endpointName);
  }

  public Task WaitForResourceHealthyAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, cancellationToken);
  }

  public Task WaitForResourceAsync(
      string resourceName,
      string state,
      CancellationToken cancellationToken = default)
  {
    return App.ResourceNotifications.WaitForResourceAsync(resourceName, state, cancellationToken);
  }

  public async Task<string?> GetConnectionStringAsync(string resourceName, CancellationToken cancellationToken = default)
  {
    return await App.GetConnectionStringAsync(resourceName, cancellationToken);
  }
}