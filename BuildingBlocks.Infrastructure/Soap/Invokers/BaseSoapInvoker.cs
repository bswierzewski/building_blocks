using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Abstractions;
using BuildingBlocks.Infrastructure.Soap.Factories;

namespace BuildingBlocks.Infrastructure.Soap.Invokers;

/// <summary>
/// Base SOAP invoker responsible for WCF client lifecycle management.
/// This is the innermost invoker in any chain - creates fresh clients, opens connections,
/// executes operations, and properly closes or aborts clients.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
/// <param name="factory">Factory for creating new client instances.</param>
public class BaseSoapInvoker<TClient>(ISoapClientFactory<TClient> factory)
    : ISoapInvoker<TClient> where TClient : ICommunicationObject
{
    /// <inheritdoc />
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        CancellationToken ct = default)
    {
        var client = factory.Create();
        try
        {
            if (client.State == CommunicationState.Created)
            {
                await Task.Factory.FromAsync(client.BeginOpen, client.EndOpen, null);
            }

            return await operation(client);
        }
        catch
        {
            if (client.State == CommunicationState.Faulted)
            {
                client.Abort();
            }
            throw;
        }
        finally
        {
            if (client.State == CommunicationState.Opened)
            {
                try
                {
                    await Task.Factory.FromAsync(client.BeginClose, client.EndClose, null);
                }
                catch
                {
                    client.Abort();
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task InvokeAsync(Func<TClient, Task> operation, CancellationToken ct = default)
    {
        await InvokeAsync<object?>(async client =>
        {
            await operation(client);
            return null;
        }, ct);
    }
}
