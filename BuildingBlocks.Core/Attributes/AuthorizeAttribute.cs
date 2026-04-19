namespace BuildingBlocks.Core.Attributes;

/// <summary>
/// Marks a Wolverine handler class or method as requiring an authenticated user.
/// Optional role and permission constraints are expressed as string arrays.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the roles required to execute the handler.
    /// Empty means no role constraint.
    /// </summary>
    public string[] Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the permissions required to execute the handler.
    /// Empty means any authenticated user.
    /// </summary>
    public string[] Permissions { get; set; } = [];
}