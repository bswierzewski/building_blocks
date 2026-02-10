using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Abstractions;
using Polly;
using Polly.Retry;

namespace BuildingBlocks.Infrastructure.Soap.Invokers;

/// <summary>
/// Resilient SOAP invoker decorator that wraps operations with retry and timeout.
/// Retry: 3 attempts with exponential backoff starting from 1s.
/// Timeout: 15s per attempt.
/// Handles: CommunicationException, TimeoutException, and non-client FaultException.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
/// <param name="inner">The inner invoker to decorate.</param>
public class ResilientSoapInvoker<TClient>(ISoapInvoker<TClient> inner)
    : ISoapInvoker<TClient> where TClient : ICommunicationObject
{
    private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(1),
            ShouldHandle = new PredicateBuilder()
                .Handle<CommunicationException>()
                .Handle<TimeoutException>()
                .Handle<FaultException>(ex => ex.Code?.Name != "Client")
        })
        .AddTimeout(TimeSpan.FromSeconds(15))
        .Build();

    /// <inheritdoc />
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        CancellationToken ct = default)
    {
        return await _pipeline.ExecuteAsync(async token =>
            await inner.InvokeAsync(operation, token), ct);
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
