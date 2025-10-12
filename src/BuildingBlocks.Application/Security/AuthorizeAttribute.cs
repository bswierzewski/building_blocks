namespace BuildingBlocks.Application.Security;

/// <summary>
/// Attribute to specify authorization requirements for MediatR requests.
/// Supports role-based, permission-based, and claim-based authorization using JWT tokens.
/// </summary>
/// <remarks>
/// <para><b>Usage Examples:</b></para>
/// <code>
/// // Require specific role
/// [Authorize(Roles = "Admin")]
/// public record DeleteUserCommand(Guid UserId) : IRequest;
///
/// // Require specific permission
/// [Authorize(Permissions = "users.delete")]
/// public record DeleteUserCommand(Guid UserId) : IRequest;
///
/// // Require multiple permissions (all required)
/// [Authorize(Permissions = "users.view, users.edit")]
/// public record UpdateUserCommand(Guid UserId, string Name) : IRequest;
///
/// // Require one of multiple roles (OR logic)
/// [Authorize(Roles = "Admin, SuperAdmin")]
/// public record DeleteUserCommand(Guid UserId) : IRequest;
///
/// // Combine roles and permissions
/// [Authorize(Roles = "Admin", Permissions = "users.delete")]
/// public record DeleteUserCommand(Guid UserId) : IRequest;
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the required roles (comma-separated).
    /// The user must have at least one of the specified roles (OR logic).
    /// </summary>
    /// <example>"Admin, SuperAdmin"</example>
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets the required permissions (comma-separated).
    /// The user must have all specified permissions (AND logic).
    /// </summary>
    /// <example>"users.view, users.edit"</example>
    public string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets the required claims (comma-separated).
    /// Format: "claimType" or "claimType:claimValue".
    /// The user must have all specified claims (AND logic).
    /// </summary>
    /// <example>"email_verified:true, country:US"</example>
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