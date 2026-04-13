using BuildingBlocks.Core.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Runs <see cref="IModule.InitializeAsync"/> for every registered module after the host starts.
/// Stops the application if any module fails to initialize.
/// </summary>
internal sealed class ModuleInitializerService(
    IModule[] modules,
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    ILogger<ModuleInitializerService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var module in modules)
        {
            try
            {
                logger.LogInformation("Initializing module {ModuleName}", module.GetType().Name);

                // Delegate all module-specific startup work (e.g. applying EF migrations,
                // seeding reference data) to the module itself. The root IServiceProvider
                // is passed so the module can create its own scope.
                await module.InitializeAsync(services, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Module {ModuleName} failed to initialize. Stopping application", module.GetType().Name);

                // Trigger an orderly shutdown before re-throwing so that other hosted services
                // have a chance to complete their StopAsync callbacks. Without this the host
                // would keep running with a partially initialised module.
                lifetime.StopApplication();
                throw;
            }
        }
    }

    // Nothing to do on shutdown — module teardown is handled by the modules' own
    // IDisposable/IAsyncDisposable registrations resolved from the DI container.
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
