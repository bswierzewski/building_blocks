using System.Security.Claims;
using BuildingBlocks.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Authentication.Users;

/// <summary>
/// Resolves the current user from the active HTTP request claims.
/// </summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    private static readonly string[] IdClaimTypes = [ClaimTypes.NameIdentifier, "sub"];
    private static readonly string[] RoleClaimTypes = [ClaimTypes.Role, "role", "roles", "org_role"];

    public string Id => IdClaimTypes
        .Select(claimType => Principal?.FindFirst(claimType)?.Value)
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?? string.Empty;

    public IEnumerable<string> Roles => Principal is null
        ? []
        : RoleClaimTypes
            .SelectMany(claimType => Principal.FindAll(claimType))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}