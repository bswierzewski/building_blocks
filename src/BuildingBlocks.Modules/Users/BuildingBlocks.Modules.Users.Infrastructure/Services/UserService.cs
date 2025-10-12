using System.Security.Claims;
using BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Modules.Users.Infrastructure.Services;

/// <summary>
/// Implementation of IUser that extracts user information from HTTP context claims.
/// All claims are standardized during token validation (OnTokenValidated event).
/// </summary>
public class UserService : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the UserService class.
    /// </summary>
    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Gets the internal unified user ID (Guid).
    /// This is added to claims during token validation after JIT provisioning.
    /// </summary>
    public string Id =>
        User?.FindFirst("user_id")?.Value
        ?? throw new UnauthorizedAccessException("User not authenticated or internal ID not set");

    /// <summary>
    /// Gets the user's email address.
    /// Uses standard ClaimTypes.Email.
    /// </summary>
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Gets the full name of the user.
    /// Uses standard ClaimTypes.Name.
    /// </summary>
    public string? FullName => User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Gets the URL to the user's profile picture.
    /// Uses the "picture" claim (not part of standard ClaimTypes).
    /// </summary>
    public string? PictureUrl => User?.FindFirst("picture")?.Value;

    /// <summary>
    /// Gets whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Gets all claim types associated with the current user.
    /// </summary>
    public IEnumerable<string> Claims => User?.Claims.Select(c => c.Type) ?? Enumerable.Empty<string>();

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// Uses standard ClaimTypes.Role populated from database during token validation.
    /// </summary>
    public IEnumerable<string> Roles
    {
        get
        {
            if (User == null) return Enumerable.Empty<string>();

            return User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct();
        }
    }

    /// <summary>
    /// Gets the permissions assigned to the current user.
    /// Uses "permission" claim populated from database during token validation.
    /// </summary>
    public IEnumerable<string> Permissions
    {
        get
        {
            if (User == null) return Enumerable.Empty<string>();

            return User.FindAll("permission")
                .Select(c => c.Value)
                .Distinct();
        }
    }

    /// <summary>
    /// Determines whether the user has the specified role.
    /// </summary>
    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the user has the specified claim.
    /// </summary>
    public bool HasClaim(string claimType, string? claimValue = null)
    {
        if (User == null) return false;

        var claims = User.FindAll(claimType);
        if (!claims.Any()) return false;

        return claimValue == null || claims.Any(c => c.Value.Equals(claimValue, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the user has the specified permission.
    /// </summary>
    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
