using System.ServiceModel;

namespace BuildingBlocks.Infrastructure.Soap.Abstractions;

/// <summary>
/// Invokes an operation on a WCF SOAP client.
/// Decorate this interface to add cross-cutting concerns (resilience, logging, caching, etc.).
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public interface ISoapInvoker<TClient> where TClient : ICommunicationObject
{
    /// <summary>
    /// Invokes an asynchronous operation on the SOAP client and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute on the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        CancellationToken ct = default);

    /// <summary>
    /// Invokes an asynchronous operation on the SOAP client without returning a result.
    /// </summary>
    /// <param name="operation">The operation to execute on the client.</param>
    /// <param name="ct">Cancellation token.</param>
    Task InvokeAsync(
        Func<TClient, Task> operation,
        CancellationToken ct = default);
}