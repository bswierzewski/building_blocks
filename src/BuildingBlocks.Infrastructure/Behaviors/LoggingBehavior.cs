using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Behaviors;

public class LoggingBehavior<TRequest>(ILogger<LoggingBehavior<TRequest>> logger) 
    : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling: {RequestName}: {@Payload}", typeof(TRequest).Name, request);

        return Task.CompletedTask;
    }
}