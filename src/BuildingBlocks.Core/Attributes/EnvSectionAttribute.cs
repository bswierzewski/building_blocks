namespace BuildingBlocks.Core.Attributes;

/// <summary>
/// Marks a class as an environment configuration section.
/// Can be used as an alternative to implementing IOptions interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EnvSectionAttribute : Attribute
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Gets the description of the configuration section.
    /// Used for generating section comments in the .env file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvSectionAttribute"/> class.
    /// </summary>
    /// <param name="sectionName">The configuration section name</param>
    public EnvSectionAttribute(string sectionName)
    {
        SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
    }
}
