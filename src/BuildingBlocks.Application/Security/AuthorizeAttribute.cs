namespace BuildingBlocks.Application.Security;

/// <summary>
/// Attribute to specify authorization requirements for MediatR requests.
/// Supports role-based and claim-based authorization using JWT tokens.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the required roles (comma-separated).
    /// The user must have at least one of the specified roles.
    /// </summary>
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets the required claims (comma-separated).
    /// Format: "claimType" or "claimType:claimValue".
    /// The user must have all specified claims.
    /// </summary>
    public string? Claims { get; set; }

    /// <summary>
    /// Gets or sets the policy name for policy-based authorization.
    /// Note: Policy-based authorization is not yet implemented.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Initializes a new instance of the AuthorizeAttribute class.
    /// </summary>
    public AuthorizeAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuthorizeAttribute class with required roles.
    /// </summary>
    /// <param name="roles">The required roles (comma-separated).</param>
    public AuthorizeAttribute(string roles)
    {
        Roles = roles;
    }
}