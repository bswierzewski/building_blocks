namespace BuildingBlocks.Tools.Models;

/// <summary>
/// Information about a class implementing IOptions.
/// </summary>
internal class OptionsClassInfo
{
    public required string ClassName { get; set; }
    public required string SectionName { get; set; }
    public required List<PropertyInfo> Properties { get; set; }
}
