namespace BuildingBlocks.Kernel.Attributes;

/// <summary>
/// Specifies authorization requirements for a message handler or HTTP endpoint.
/// Used by Wolverine middleware to enforce role-based access control.
/// </summary>
/// <param name="roles">Required roles for authorization</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizeAttribute(params string[] roles) : Attribute
{
    /// <summary>
    /// Gets the collection of roles required to execute the handler.
    /// User must have at least one of these roles.
    /// </summary>
    public IReadOnlyList<string> Roles { get; } = roles;
}
