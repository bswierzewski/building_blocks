using System.Security.Claims;
using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Reads the current user context directly from JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private static readonly string[] UserIdClaimTypes = [ClaimTypes.NameIdentifier, "sub"];
    private static readonly string[] EmailClaimTypes = [ClaimTypes.Email, "email"];
    private static readonly string[] RoleClaimTypes = [ClaimTypes.Role, "role", "roles"];

    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public Guid Id => Guid.Empty;

    public string Email => string.Empty;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles => [];
}