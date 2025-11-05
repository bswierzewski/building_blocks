using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.Enums;

namespace BuildingBlocks.Modules.Users.Application.Abstractions;

/// <summary>
/// Service responsible for Just-In-Time (JIT) user provisioning.
/// Handles the creation, retrieval, and updates of users during authentication.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>
    /// Creates a new user or updates an existing one (upsert operation).
    /// If the user doesn't exist, creates them with the provided information.
    /// If the user exists, updates their profile and last login timestamp.
    /// </summary>
    /// <param name="provider">The identity provider</param>
    /// <param name="externalUserId">The external user ID from the provider (e.g., JWT 'sub' claim)</param>
    /// <param name="email">User's email address</param>
    /// <param name="displayName">User's display name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user with their roles and permissions loaded</returns>
    Task<User> UpsertUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default);
}
