namespace BuildingBlocks.Kernel.Abstractions;

/// <summary>
/// Marker interface for strongly-typed configuration options with a static section name
/// </summary>
public interface IOptions
{
    /// <summary>
    /// Gets the configuration section name for this options class
    /// </summary>
    static abstract string SectionName { get; }
}