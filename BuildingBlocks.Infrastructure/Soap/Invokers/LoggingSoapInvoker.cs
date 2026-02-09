using System.Diagnostics;
using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Soap.Invokers;

/// <summary>
/// Logging SOAP invoker decorator that logs operation start, completion, and failures.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
/// <param name="inner">The inner invoker to decorate.</param>
/// <param name="logger">Logger instance.</param>
public class LoggingSoapInvoker<TClient>(
    ISoapInvoker<TClient> inner,
    ILogger<LoggingSoapInvoker<TClient>> logger)
    : ISoapInvoker<TClient> where TClient : ICommunicationObject
{
    private static readonly string ClientName = typeof(TClient).Name;

    /// <inheritdoc />
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        CancellationToken ct = default)
    {
        logger.LogInformation("SOAP {ClientName} — invoking", ClientName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await inner.InvokeAsync(operation, ct);
            stopwatch.Stop();
            logger.LogInformation("SOAP {ClientName} — completed in {ElapsedMs}ms", ClientName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "SOAP {ClientName} — failed after {ElapsedMs}ms", ClientName, stopwatch.ElapsedMilliseconds);
            throw;
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
