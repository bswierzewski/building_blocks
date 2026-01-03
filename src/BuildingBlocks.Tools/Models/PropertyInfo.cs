namespace BuildingBlocks.Tools.Models;

/// <summary>
/// Information about a property in an options class.
/// </summary>
internal class PropertyInfo
{
    public required string Name { get; set; }
    public required string TypeName { get; set; }
    public string? DefaultValue { get; set; }
}
