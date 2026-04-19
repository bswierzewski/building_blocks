namespace BuildingBlocks.Core.Attributes;

/// <summary>
/// Marks a Wolverine handler class or method as requiring an authenticated user.
/// Optional permission constraints are expressed as a comma-delimited list.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a comma-delimited list of permissions required to execute the handler.
    /// Empty means any authenticated user.
    /// </summary>
    public string Permissions { get; set; } = string.Empty;
}