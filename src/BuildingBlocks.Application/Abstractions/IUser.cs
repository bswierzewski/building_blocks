namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Interface representing the current authenticated user.
/// Provides access to user information extracted from JWT tokens (Auth0, Clerk, etc.).
/// </summary>
public interface IUser
{
    /// <summary>
    /// Gets the unique identifier of the user from the JWT subject claim.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the email address of the user from JWT claims.
    /// Can be null if email claim is not present in the token.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// Returns true if a valid JWT token is present.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets all claims associated with the current user from the JWT token.
    /// </summary>
    IEnumerable<string> Claims { get; }

    /// <summary>
    /// Gets the roles assigned to the current user from JWT claims.
    /// Roles are typically stored in role or roles claims depending on the provider.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Determines whether the user has the specified role.
    /// </summary>
    /// <param name="role">The role to check for.</param>
    /// <returns>True if the user has the specified role; otherwise, false.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Determines whether the user has the specified claim.
    /// </summary>
    /// <param name="claimType">The type of the claim to check for.</param>
    /// <param name="claimValue">The value of the claim to check for. If null, checks only for claim type existence.</param>
    /// <returns>True if the user has the specified claim; otherwise, false.</returns>
    bool HasClaim(string claimType, string? claimValue = null);
}