using System.Security.Claims;
using BuildingBlocks.Identity.Configuration;
using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Identity.Domain.Enums;
using BuildingBlocks.Identity.Infrastructure.Authentication;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Identity.Infrastructure.Authentication.Services;

/// <summary>
/// Creates or links a local identity record for an authenticated external principal
/// and enriches the principal with local authorization claims.
/// </summary>
public sealed class JitProvisioningService(IdentityDbContext dbContext, IOptions<JitProvisioningOptions> options)
{
    /// <summary>
    /// Resolves the local user for an external identity and projects local claims onto the principal.
    /// </summary>
    public async Task ProvisionUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        // Providers normalize their token claims into the shared internal claim set before invoking JIT.
        var provider = GetExternalProvider(principal);
        var externalId = principal.FindFirst(IdentityClaimTypes.ExternalUserId)?.Value;
        if (string.IsNullOrWhiteSpace(externalId))
            return;

        var email = principal.FindFirst(IdentityClaimTypes.Email)?.Value;
        if (principal.Identity is not ClaimsIdentity identity)
            return;

        var user = await GetOrCreateUserAsync(provider, externalId, email, cancellationToken);

        // Keep the provider subject intact and expose the local database identifier separately.
        AddClaim(identity, IdentityClaimTypes.LocalUserId, user.Id.ToString());

        foreach (var role in user.Roles)
        {
            AddClaim(identity, ClaimTypes.Role, role.Name);

            foreach (var permission in role.Permissions)
                AddClaim(identity, IdentityClaimTypes.Permission, permission);
        }
    }

    /// <summary>
    /// Finds a user by linked external account, falls back to email matching, and creates a new user when necessary.
    /// </summary>
    private async Task<User> GetOrCreateUserAsync(ExternalProvider provider, string externalId, string? email, CancellationToken cancellationToken)
    {
        // Returning users usually hit this query and finish in one round-trip.
        var user = await dbContext.Users
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(
                x => x.ExternalAccounts.Any(account => account.Provider == provider && account.ExternalId == externalId),
                cancellationToken);

        if (user is not null)
            return user;

        // First SSO login may need to match an existing local account by email.
        if (!string.IsNullOrWhiteSpace(email))
        {
            user = await dbContext.Users
                .Include(x => x.ExternalAccounts)
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        if (user is not null)
        {
            // Email matching can find a pre-existing local account that still needs an external login attached.
            if (!user.ExternalAccounts.Any(account => account.Provider == provider && account.ExternalId == externalId))
                user.LinkExternalAccount(provider, externalId);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Cannot provision a local user without an email claim.");

            user = User.Create(provider, email, externalId, options.Value.RegistrationMode);
            dbContext.Users.Add(user);
        }

        // EF Core persists only actual changes, so a single save is enough for both create and link flows.
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <summary>
    /// Reads the normalized provider claim prepared by the JWT handler.
    /// </summary>
    private static ExternalProvider GetExternalProvider(ClaimsPrincipal principal)
    {
        var providerValue = principal.FindFirst(IdentityClaimTypes.ExternalProvider)?.Value;
        if (Enum.TryParse<ExternalProvider>(providerValue, ignoreCase: true, out var provider))
            return provider;

        throw new InvalidOperationException("The external provider claim is missing or invalid.");
    }

    /// <summary>
    /// Adds a claim only when it is not already present on the identity.
    /// </summary>
    private static void AddClaim(ClaimsIdentity identity, string claimType, string value)
    {
        if (identity.HasClaim(claimType, value))
            return;

        identity.AddClaim(new Claim(claimType, value));
    }
}