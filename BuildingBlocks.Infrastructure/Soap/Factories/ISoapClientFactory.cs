using System.ServiceModel;

namespace BuildingBlocks.Infrastructure.Soap.Factories;

/// <summary>
/// Creates a new WCF client instance on every call.
/// Critical: WCF clients in Faulted state are dead and cannot be reused.
/// Each retry attempt needs a brand new client instance.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public interface ISoapClientFactory<TClient> where TClient : ICommunicationObject
{
    /// <summary>
    /// Creates a new instance of the WCF client.
    /// </summary>
    /// <returns>A new WCF client instance.</returns>
    TClient Create();
}
