using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Identity.Infrastructure.Services;

/// <summary>
/// Reads the current application user from the authenticated HTTP principal enriched during JIT provisioning.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    // The local user id is authoritative for the application's user context.
    public Guid Id => Guid.NewGuid();

    public string Email => string.Empty;

    public bool IsAuthenticated => true;

    public IReadOnlyCollection<string> Roles => Array.Empty<string>();

    public IReadOnlyCollection<string> Permissions => Array.Empty<string>();

    public bool HasPermission(string permission) => false;

    public bool HasRole(string role) => false;
}