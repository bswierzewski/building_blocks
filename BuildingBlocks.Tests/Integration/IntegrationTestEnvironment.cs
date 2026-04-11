using Alba;
using BuildingBlocks.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

public abstract class IntegrationTestEnvironment : IAsyncLifetime
{
  private static readonly SemaphoreSlim EnvironmentVariablesLock = new(1, 1);

  public abstract Task<IAlbaHost> CreateHostAsync(Action<IServiceCollection>? configureServices = null);

  public abstract Task ResetDatabaseAsync();

  public abstract Task InitializeAsync();

  public abstract Task DisposeAsync();

  protected static async Task<IDisposable> ApplyEnvironmentVariablesAsync(
      IReadOnlyDictionary<string, string?> environmentVariables)
  {
    await EnvironmentVariablesLock.WaitAsync();

    var previousValues = new Dictionary<string, string?>(StringComparer.Ordinal);

    foreach (var (key, value) in environmentVariables)
    {
      previousValues[key] = Environment.GetEnvironmentVariable(key);
      Environment.SetEnvironmentVariable(key, value);
    }

    return new EnvironmentVariablesScope(previousValues);
  }

  private sealed class EnvironmentVariablesScope(
      IReadOnlyDictionary<string, string?> previousValues) : IDisposable
  {
    public void Dispose()
    {
      foreach (var (key, value) in previousValues)
      {
        Environment.SetEnvironmentVariable(key, value);
      }

      EnvironmentVariablesLock.Release();
    }
  }
}

public abstract class IntegrationTestEnvironment<TProgram> : IntegrationTestEnvironment
    where TProgram : class
{
  private const string DefaultConnectionStringEnvironmentVariable = "ConnectionStrings__Default";
  private readonly DatabaseFixture _databaseFixture = new();

  protected virtual string EnvironmentName => "Testing";

  protected virtual string ConnectionStringEnvironmentVariable => DefaultConnectionStringEnvironmentVariable;

  protected virtual string ConnectionString => _databaseFixture.ConnectionString;

  protected virtual IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
  {
    return new Dictionary<string, string?>(StringComparer.Ordinal)
    {
      [ConnectionStringEnvironmentVariable] = ConnectionString
    };
  }

  public override Task InitializeAsync()
  {
    return _databaseFixture.InitializeAsync();
  }

  public override Task ResetDatabaseAsync()
  {
    return _databaseFixture.ResetDatabaseAsync();
  }

  public override Task DisposeAsync()
  {
    return _databaseFixture.DisposeAsync();
  }

  public override async Task<IAlbaHost> CreateHostAsync(Action<IServiceCollection>? configureServices = null)
  {
    using var _ = await ApplyEnvironmentVariablesAsync(GetEnvironmentVariables());

    return await AlbaHost.For<TProgram>(builder =>
    {
      builder.UseEnvironment(EnvironmentName);

      builder.ConfigureLogging(logging =>
          {
          logging.ClearProviders();
          logging.AddConsole();
          logging.SetMinimumLevel(LogLevel.Warning);
        });

      ConfigureHost(builder);

      builder.ConfigureServices((_, services) =>
          {
          services.AddSingleton(TimeProvider.System);
          ConfigureEnvironmentServices(services);
          configureServices?.Invoke(services);
        });
    });
  }

  protected virtual void ConfigureHost(IWebHostBuilder builder)
  {
  }

  protected virtual void ConfigureEnvironmentServices(IServiceCollection services)
  {
  }
}