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
public class UserProvisioningService : IUserProvisioningService
{
    private readonly IUsersWriteDbContext _writeContext;
    private readonly IUsersReadDbContext _readContext;
    private readonly ILogger<UserProvisioningService> _logger;

    /// <summary>
    /// Initializes a new instance of the UserProvisioningService class.
    /// </summary>
    public UserProvisioningService(
        IUsersWriteDbContext writeContext,
        IUsersReadDbContext readContext,
        ILogger<UserProvisioningService> logger)
    {
        _writeContext = writeContext;
        _readContext = readContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserAsync(
        IdentityProvider provider,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        var user = await _readContext.Users
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
            _logger.LogDebug(
                "User {UserId} authenticated via {Provider}",
                user.Id.Value, provider);
        }

        return user;
    }

    /// <inheritdoc />
    public async Task<User> AddUserAsync(
        IdentityProvider provider,
        string externalUserId,
        string? email,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("External user ID cannot be empty", nameof(externalUserId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required for user provisioning", nameof(email));

        var userId = UserId.Create();
        var userEmail = Email.Create(email);

        var user = User.Create(userId, userEmail, displayName);
        user.LinkExternalIdentity(provider, externalUserId);

        await _writeContext.Users.AddAsync(user, cancellationToken);
        await _writeContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "JIT provisioned new user {UserId} from {Provider}:{ExternalId} with email {Email}",
            userId.Value, provider, externalUserId, email);

        return user;
    }
}
