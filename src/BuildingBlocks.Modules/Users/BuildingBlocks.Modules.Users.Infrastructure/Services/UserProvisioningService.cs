using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.Enums;
using BuildingBlocks.Modules.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Modules.Users.Infrastructure.Services;

/// <summary>
/// Service responsible for Just-In-Time (JIT) user provisioning.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserProvisioningService class.
/// </remarks>
public class UserProvisioningService(
    IUsersWriteDbContext writeContext,
    IUsersReadDbContext readContext,
    ILogger<UserProvisioningService> logger) : IUserProvisioningService
{

    /// <inheritdoc />
    public async Task<User> UpsertUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        // Try to get existing user by external identity
        var user = await GetUserByExternalIdAsync(provider, externalUserId, cancellationToken);

        if (user == null)
        {
            // Try to find existing user by email (for linking multiple providers)
            user = await GetUserByEmailAsync(email, cancellationToken);

            if (user != null)
            {
                // User exists with this email - link new external identity
                await LinkExternalIdentityAsync(user, provider, externalUserId, cancellationToken);
                logger.LogInformation($"Linked {provider} external identity to existing user {user.Id.Value} with email {email}");
            }
            else
            {
                // Create new user (JIT provisioning)
                user = await CreateUserAsync(provider, externalUserId, email, displayName, cancellationToken);
            }
        }

        // Update existing user if profile changed
        await UpdateUserAsync(user, displayName, cancellationToken);

        return user;
    }

    private async Task<User?> GetUserByExternalIdAsync(
        IdentityProvider provider,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        var user = await readContext.Users
            .Include(u => u.ExternalIdentities)
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u =>
                u.ExternalIdentities.Any(e =>
                    e.Provider == provider &&
                    e.ExternalUserId == externalUserId),
                cancellationToken);

        if (user != null)
        {
            logger.LogDebug(
                "User {UserId} authenticated via {Provider}",
                user.Id.Value, provider);
        }

        return user;
    }

    private async Task<User?> GetUserByEmailAsync(
        string? email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Create Email value object to compare with database (EF Core will handle conversion)
        var emailVO = Email.Create(email);

        // Query by Email value object - EF Core HasConversion will handle the comparison
        var user = await readContext.Users
            .Include(u => u.ExternalIdentities)
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == emailVO, cancellationToken);

        if (user != null)
        {
            logger.LogDebug(
                "Found existing user {UserId} with email {Email}",
                user.Id.Value, email);
        }

        return user;
    }

    private async Task LinkExternalIdentityAsync(
        User user,
        IdentityProvider provider,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        // Check if this external identity already exists
        if (user.ExternalIdentities.Any(e => e.Provider == provider && e.ExternalUserId == externalUserId))
        {
            logger.LogDebug(
                "External identity {Provider}:{ExternalId} already linked to user {UserId}",
                provider, externalUserId, user.Id.Value);
            return;
        }

        user.LinkExternalIdentity(provider, externalUserId);
        await writeContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> CreateUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required for user provisioning", nameof(email));

        var userId = UserId.Create();
        var userEmail = Email.Create(email);

        var user = User.Create(userId, userEmail, displayName);
        user.LinkExternalIdentity(provider, externalUserId);

        await writeContext.Users.AddAsync(user, cancellationToken);
        await writeContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "JIT provisioned new user {UserId} from {Provider}:{ExternalId} with email {Email}",
            userId.Value, provider, externalUserId, email);

        return user;
    }

    private async Task UpdateUserAsync(User user, string? displayName, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Update profile if displayName has changed
        if (user.DisplayName != displayName)
        {
            var oldDisplayName = user.DisplayName;
            user.UpdateProfile(user.Email, displayName);

            await writeContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated user {UserId} profile - DisplayName: '{OldName}' -> '{NewName}'",
                user.Id.Value, oldDisplayName, displayName);
        }

        // Update last login timestamp
        user.UpdateLastLogin();
        await writeContext.SaveChangesAsync(cancellationToken);
    }
}
