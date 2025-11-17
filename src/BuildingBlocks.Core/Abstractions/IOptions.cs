namespace BuildingBlocks.Core.Abstractions;

/// <summary>
/// Interface for configuration options classes.
/// Implement this interface to define configuration sections that can be scanned and exported.
/// </summary>
public interface IOptions
{
    /// <summary>
    /// Gets the configuration section name (e.g., "Database", "Auth", "RabbitMQ").
    /// This is used as the prefix for environment variables.
    /// </summary>
    static abstract string SectionName { get; }
}
