using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class HostApplicationLifetimeExtensions
{
    public static async Task<bool> WaitForAppStartupAsync(
        this IHostApplicationLifetime lifetime,
        CancellationToken stoppingToken)
    {
        if (lifetime.ApplicationStarted.IsCancellationRequested)        
            return true;        

        var tcs = new TaskCompletionSource();

        using var registration = lifetime.ApplicationStarted.Register(() => tcs.TrySetResult());

        var cancellationTask = Task.Delay(Timeout.Infinite, stoppingToken);

        var completedTask = await Task.WhenAny(tcs.Task, cancellationTask);

        return completedTask == tcs.Task;
    }
}
