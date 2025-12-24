using BuildingBlocks.Abstractions.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Infrastructure.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IUserContext userContext) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public const int PerformanceThresholdMs = 500;

    public const int CriticalPerformanceThresholdMs = 2000;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;

        // Log performance metrics based on elapsed time
        if (elapsedMilliseconds > CriticalPerformanceThresholdMs)
        {
            logger.LogError(
                "CRITICAL PERFORMANCE: Request {RequestName} took {ElapsedMilliseconds}ms to complete. " +
                "User: {UserId}. Request: {@Request}",
                requestName,
                elapsedMilliseconds,
                userContext.Id.ToString(),
                request);
        }
        else if (elapsedMilliseconds > PerformanceThresholdMs)
        {
            logger.LogWarning(
                "SLOW REQUEST: Request {RequestName} took {ElapsedMilliseconds}ms to complete. " +
                "User: {UserId}",
                requestName,
                elapsedMilliseconds,
                userContext.Id.ToString());

            // Log request details only in debug mode for performance warnings
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Slow request {RequestName} details: {@Request}",
                    requestName,
                    request);
            }
        }
        else if (logger.IsEnabled(LogLevel.Debug))
        {
            // Log fast requests only in debug mode
            logger.LogDebug(
                "Request {RequestName} completed in {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }

        // Log performance metrics for structured logging and monitoring
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["ElapsedMilliseconds"] = elapsedMilliseconds,
            ["UserId"] = userContext.Id.ToString(),
            ["IsSlowRequest"] = elapsedMilliseconds > PerformanceThresholdMs,
            ["IsCriticalPerformance"] = elapsedMilliseconds > CriticalPerformanceThresholdMs
        }))
        {
            logger.LogInformation(
                "Performance metrics for {RequestName}: {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }

        return response;
    }
}