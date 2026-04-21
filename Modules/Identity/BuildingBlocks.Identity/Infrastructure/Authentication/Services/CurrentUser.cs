using BuildingBlocks.Core.Interfaces;
using System.Security.Claims;
using BuildingBlocks.Identity.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Identity.Infrastructure.Authentication.Services;

/// <summary>
/// Reads the current application user from the authenticated HTTP principal enriched during JIT provisioning.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    // Lazily cache claim parsing because the same values can be requested multiple times during one request.
    private readonly Lazy<Guid> _id = new(() =>
        Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirst(IdentityClaimTypes.LocalUserId)?.Value,
            out var id)
            ? id
            : Guid.Empty);

    private readonly Lazy<string> _email = new(() =>
        httpContextAccessor.HttpContext?.User.FindFirst(IdentityClaimTypes.Email)?.Value
        ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
        ?? string.Empty);

    private readonly Lazy<HashSet<string>> _roles = new(() =>
        new HashSet<string>(
            httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value) ?? [],
            StringComparer.Ordinal));

    private readonly Lazy<HashSet<string>> _permissions = new(() =>
        new HashSet<string>(
            httpContextAccessor.HttpContext?.User.FindAll(IdentityClaimTypes.Permission).Select(claim => claim.Value) ?? [],
            StringComparer.Ordinal));

    // The local user id is authoritative for the application's user context.
    public Guid Id => _id.Value;

    public string Email => _email.Value;

    public bool IsAuthenticated => Id != Guid.Empty;

    public IReadOnlyCollection<string> Roles => _roles.Value;

    public IReadOnlyCollection<string> Permissions => _permissions.Value;

    public bool HasPermission(string permission)
        => _permissions.Value.Contains(permission);

    public bool HasRole(string role)
        => _roles.Value.Contains(role);
}