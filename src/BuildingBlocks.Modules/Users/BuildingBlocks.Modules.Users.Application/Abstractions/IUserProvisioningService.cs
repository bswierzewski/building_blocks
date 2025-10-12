using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.Enums;

namespace BuildingBlocks.Modules.Users.Application.Abstractions;

/// <summary>
/// Service responsible for Just-In-Time (JIT) user provisioning.
/// Handles the creation and retrieval of users during authentication.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>
    /// Gets an existing user by their external identity.
    /// Returns null if user is not found.
    /// </summary>
    /// <param name="provider">The identity provider</param>
    /// <param name="externalUserId">The external user ID from the provider (e.g., JWT 'sub' claim)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user with their roles and permissions loaded, or null if not found</returns>
    Task<User?> GetUserAsync(
        IdentityProvider provider,
        string externalUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user with the specified external identity.
    /// Used for Just-In-Time (JIT) provisioning during first authentication.
    /// </summary>
    /// <param name="provider">The identity provider</param>
    /// <param name="externalUserId">The external user ID from the provider (e.g., JWT 'sub' claim)</param>
    /// <param name="email">User's email address</param>
    /// <param name="displayName">User's display name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created user</returns>
    Task<User> AddUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default);
}
