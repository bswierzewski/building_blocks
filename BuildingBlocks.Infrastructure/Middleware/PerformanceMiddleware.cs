using System.Diagnostics;
using BuildingBlocks.Core.Interfaces;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Configuration;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that measures handler execution time using a <see cref="Stopwatch"/>.
/// A warning is emitted whenever the elapsed time exceeds <see cref="SlowRequestThresholdMs"/>.
/// Wolverine discovers <see cref="Before"/> and <see cref="After"/> by convention (method names).
/// </summary>
public static class PerformanceMiddleware
{
    /// <summary>Threshold in milliseconds above which a handler is considered slow.</summary>
    private const long SlowRequestThresholdMs = 500;

    /// <summary>
    /// Executed before the message handler. Starts a new <see cref="Stopwatch"/> and returns it
    /// so Wolverine can inject it into <see cref="After"/> via its generated code.
    /// </summary>
    public static Stopwatch Before()
    {
        var timer = new Stopwatch();
        timer.Start();
        return timer;
    }

    /// <summary>
    /// Executed after the message handler. Stops the timer and logs a warning when the elapsed
    /// time exceeds the slow-request threshold, including the handler name and the current user ID.
    /// </summary>
    public static void After(
        Stopwatch timer,
        ILogger logger,
        IMessageContext context,
        ICurrentUser currentUser)
    {
        timer.Stop();

        // Skip logging if the request completed within the acceptable threshold.
        if (timer.ElapsedMilliseconds <= SlowRequestThresholdMs)
            return;

        var requestName = context.Envelope?.Message?.GetType().Name ?? "Unknown";

        logger.LogWarning(
            "Long Running Request: {Name} ({ElapsedMilliseconds} ms) {UserId}",
            requestName,
            timer.ElapsedMilliseconds,
            currentUser.Id);
    }
}

/// <summary>
/// Tracks handler execution time and logs a warning when it exceeds 500 ms.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PerformanceAttribute : ModifyChainAttribute
{
    /// <summary>
    /// Inserts <see cref="PerformanceMiddleware.Before"/> at the start of the pipeline
    /// and appends <see cref="PerformanceMiddleware.After"/> as a postprocessor so it
    /// always runs after the handler, even when an exception is thrown.
    /// </summary>
    public override void Modify(IChain chain, GenerationRules rules, IServiceContainer container)
    {
        chain.Middleware.Insert(0, new MethodCall(typeof(PerformanceMiddleware), nameof(PerformanceMiddleware.Before)));
        chain.Postprocessors.Add(new MethodCall(typeof(PerformanceMiddleware), nameof(PerformanceMiddleware.After)));
    }
}
