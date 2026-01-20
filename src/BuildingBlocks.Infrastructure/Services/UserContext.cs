using BuildingBlocks.Kernel.Abstractions;
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
            var idClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return idClaim != null && Guid.TryParse(idClaim, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public IEnumerable<string> Roles => User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        ?? [];

    public bool IsInRole(string role) 
        => User?.IsInRole(role) ?? false;
}
