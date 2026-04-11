using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Core.Abstractions;

/// <summary>
/// Contract for application modules that can register services and expose runtime metadata.
/// </summary>
public interface IModule
{
  /// <summary>
  /// Logical module name used for diagnostics and configuration grouping.
  /// </summary>
  static abstract string Name { get; }

  /// <summary>
  /// Registers module services in the DI container.
  /// </summary>
  void AddServices(IServiceCollection services, IConfiguration configuration);

  /// <summary>
  /// Performs startup initialization after the host has been built.
  /// </summary>
  Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
