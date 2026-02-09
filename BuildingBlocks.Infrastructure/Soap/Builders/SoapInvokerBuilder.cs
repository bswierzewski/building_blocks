using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Abstractions;
using BuildingBlocks.Infrastructure.Soap.Factories;
using BuildingBlocks.Infrastructure.Soap.Invokers;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Soap.Builders;

/// <summary>
/// Configuration holder for cache duration per client type.
/// </summary>
public record CacheDurationConfig<TClient>(TimeSpan Duration) where TClient : ICommunicationObject;

/// <summary>
/// Builder for configuring SOAP invoker decorator chain.
/// Ensures decorators are applied in the correct order.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public class SoapInvokerBuilder<TClient> where TClient : ICommunicationObject
{
    private readonly IServiceCollection _services;
    private readonly List<Type> _decoratorTypes = [];
    private bool _built;

    internal SoapInvokerBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Adds resilience decorator (retry and timeout) to the invoker chain.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public SoapInvokerBuilder<TClient> AddResilience()
    {
        EnsureNotBuilt();
        _decoratorTypes.Add(typeof(ResilientSoapInvoker<TClient>));
        return this;
    }

    /// <summary>
    /// Adds logging decorator to the invoker chain.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public SoapInvokerBuilder<TClient> AddLogging()
    {
        EnsureNotBuilt();
        _decoratorTypes.Add(typeof(LoggingSoapInvoker<TClient>));
        return this;
    }

    /// <summary>
    /// Adds caching decorator to the invoker chain.
    /// </summary>
    /// <param name="cacheDuration">Time-to-live for cached entries.</param>
    /// <returns>The builder for chaining.</returns>
    public SoapInvokerBuilder<TClient> AddCache(TimeSpan cacheDuration)
    {
        EnsureNotBuilt();
        _services.AddSingleton(new CacheDurationConfig<TClient>(cacheDuration));
        _decoratorTypes.Add(typeof(CachingSoapInvoker<TClient>));
        return this;
    }

    /// <summary>
    /// Adds a custom invoker decorator to the chain.
    /// The decorator type must have a constructor accepting ISoapInvoker&lt;TClient&gt; as the first parameter.
    /// </summary>
    /// <typeparam name="TInvoker">The custom invoker type.</typeparam>
    /// <returns>The builder for chaining.</returns>
    public SoapInvokerBuilder<TClient> Add<TInvoker>() where TInvoker : ISoapInvoker<TClient>
    {
        EnsureNotBuilt();
        _decoratorTypes.Add(typeof(TInvoker));
        return this;
    }

    /// <summary>
    /// Builds and registers the complete invoker chain with dependency injection.
    /// Decorators are applied in the order they were added: Base → first decorator → ... → last decorator.
    /// </summary>
    /// <returns>The service collection for further configuration.</returns>
    public IServiceCollection Build()
    {
        if (_built)
        {
            return _services;
        }

        _built = true;

        _services.AddScoped<ISoapInvoker<TClient>>(sp =>
        {
            // Start with BaseSoapInvoker (innermost - handles lifecycle)
            var factory = sp.GetRequiredService<ISoapClientFactory<TClient>>();
            ISoapInvoker<TClient> invoker = new BaseSoapInvoker<TClient>(factory);

            // Apply each decorator in order
            foreach (var decoratorType in _decoratorTypes)
            {
                invoker = (ISoapInvoker<TClient>)ActivatorUtilities.CreateInstance(
                    sp, decoratorType, invoker);
            }

            return invoker;
        });

        return _services;
    }

    private void EnsureNotBuilt()
    {
        if (_built)
        {
            throw new InvalidOperationException(
                "Cannot modify SoapInvokerBuilder after Build() has been called.");
        }
    }
}
