using System.ServiceModel;
using BuildingBlocks.Infrastructure.Soap.Abstractions;
using BuildingBlocks.Infrastructure.Soap.Builders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Soap.Invokers;

/// <summary>
/// Caching SOAP invoker decorator that caches operation results.
/// Cache is per-client type with configurable TTL.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
/// <param name="inner">The inner invoker to decorate.</param>
/// <param name="memoryCache">Memory cache instance.</param>
/// <param name="logger">Logger instance.</param>
/// <param name="config">Cache duration configuration.</param>
public class CachingSoapInvoker<TClient>(
    ISoapInvoker<TClient> inner,
    IMemoryCache memoryCache,
    ILogger<CachingSoapInvoker<TClient>> logger,
    CacheDurationConfig<TClient> config)
    : ISoapInvoker<TClient> where TClient : ICommunicationObject
{
    private static readonly string CacheKey = $"soap_cache_{typeof(TClient).Name}";
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = config.Duration,
        Priority = CacheItemPriority.High,
        Size = 1
    };

    /// <inheritdoc />
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        CancellationToken ct = default)
    {
        // Try to get from cache
        if (memoryCache.TryGetValue<TResult>(CacheKey, out var cachedResult))
        {
            logger.LogDebug("SOAP {ClientName} — cache hit", typeof(TClient).Name);
            return cachedResult!;
        }

        // Cache miss - invoke and cache result
        logger.LogDebug("SOAP {ClientName} — cache miss, invoking", typeof(TClient).Name);
        var result = await inner.InvokeAsync(operation, ct);

        memoryCache.Set(CacheKey, result, _cacheOptions);
        logger.LogInformation("SOAP {ClientName} — result cached for {Duration}",
            typeof(TClient).Name, config.Duration);

        return result;
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
