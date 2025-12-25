using BuildingBlocks.Abstractions.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid Id
    {
        get
        {
            var subClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("sub")?.Value;

            return subClaim != null && Guid.TryParse(subClaim, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string Email => User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value
        ?? string.Empty;

    public IEnumerable<string> Roles => User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        ?? [];

    public bool IsInRole(string role) 
        => User?.IsInRole(role) ?? false;
}
