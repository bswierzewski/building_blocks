using BuildingBlocks.Kernel.Attributes;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace BuildingBlocks.Infrastructure.Middleware;

/// <summary>
/// Wolverine middleware that logs message payload before handler execution.
/// Optional - only add to handlers that need detailed payload logging.
///
/// Usage: Add [PayloadLogging] attribute to your handler class or method.
/// This middleware is only activated when the attribute is present.
///
/// Wolverine detects this middleware by the "Before" method name convention
/// and presence of the PayloadLoggingAttribute parameter.
/// </summary>
public static class LoggingMiddleware
{
    /// <summary>
    /// Logs the message payload before executing the handler.
    /// Wolverine generates optimized code for this at compile-time.
    /// Only executed when PayloadLoggingAttribute is present on the handler.
    /// </summary>
    public static void Before(
        PayloadLoggingAttribute _,
        ILogger logger,
        IMessageContext messageContext)
    {
        var message = messageContext.Envelope?.Message;
        var messageType = message?.GetType().Name ?? "Unknown";

        logger.LogInformation("Handling: {MessageType}: {@Payload}", messageType, message);
    }
}
