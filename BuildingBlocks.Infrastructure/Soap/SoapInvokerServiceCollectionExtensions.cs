using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Builders;
using BuildingBlocks.Infrastructure.Soap.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Soap;

/// <summary>
/// Extension methods for registering SOAP invokers with dependency injection.
/// </summary>
public static class SoapInvokerServiceCollectionExtensions
{
    /// <summary>
    /// Registers a SOAP invoker for the specified WCF client type.
    /// Returns a builder to configure the decorator chain (resilience, logging, custom decorators).
    /// </summary>
    /// <typeparam name="TClient">The WCF client type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="clientFactory">Factory function that creates new WCF client instances.</param>
    /// <returns>A builder to configure the invoker chain. Call Build() to finalize registration.</returns>
    /// <example>
    /// <code>
    /// services.AddSoapInvoker&lt;MyWcfClient&gt;(() => new MyWcfClient(...))
    ///     .AddResilience()
    ///     .AddLogging()
    ///     .Build();
    /// </code>
    /// </example>
    public static SoapInvokerBuilder<TClient> AddSoapInvoker<TClient>(
        this IServiceCollection services,
        Func<TClient> clientFactory)
        where TClient : ICommunicationObject
    {
        // Register the factory as singleton (factory function is stateless)
        services.AddSingleton<ISoapClientFactory<TClient>>(
            _ => new SoapClientFactory<TClient>(clientFactory));

        return new SoapInvokerBuilder<TClient>(services);
    }
}
