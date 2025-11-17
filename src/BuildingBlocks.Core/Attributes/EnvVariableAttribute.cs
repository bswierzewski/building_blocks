namespace BuildingBlocks.Core.Attributes;

/// <summary>
/// Marks a property as an environment variable configuration field.
/// Used by the env generator tool to create .env files from options classes.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class EnvVariableAttribute : Attribute
{
    /// <summary>
    /// Gets the environment variable name.
    /// If not specified, it will be auto-generated from SectionName and property name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the description of the environment variable.
    /// Used for generating comments in the .env file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the default value for the environment variable.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets a value indicating whether this environment variable is required.
    /// </summary>
    public bool Required { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether this is a sensitive value (e.g., password, API key).
    /// Sensitive values will be marked with a placeholder in the generated .env file.
    /// </summary>
    public bool Sensitive { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvVariableAttribute"/> class.
    /// </summary>
    public EnvVariableAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvVariableAttribute"/> class with a custom name.
    /// </summary>
    /// <param name="name">Custom environment variable name</param>
    public EnvVariableAttribute(string name)
    {
        Name = name;
    }
}
