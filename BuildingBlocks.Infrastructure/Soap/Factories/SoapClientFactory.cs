using System.ServiceModel;

namespace BuildingBlocks.Infrastructure.Soap.Factories;

/// <summary>
/// Default implementation of <see cref="ISoapClientFactory{TClient}"/>.
/// Creates WCF client instances using the provided factory function.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
/// <param name="factory">Factory function that creates new client instances.</param>
public class SoapClientFactory<TClient>(Func<TClient> factory)
    : ISoapClientFactory<TClient> where TClient : ICommunicationObject
{
    /// <inheritdoc />
    public TClient Create() => factory();
}
