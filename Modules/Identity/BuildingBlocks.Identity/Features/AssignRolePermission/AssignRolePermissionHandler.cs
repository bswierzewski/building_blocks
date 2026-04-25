using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.AssignRolePermission;

public sealed class AssignRolePermissionHandler
{
    [WolverinePut("/identity/roles/{id}/permissions/{permissionCode}")]
    [Tags("Identity")]
    [EndpointName("AssignRolePermission")]
    [EndpointSummary("Assign permission to role")]
    public static async Task Handle(
        Guid id,
        string permissionCode,
        IdentityDbContext dbContext,
        IEnumerable<IModule> modules,
        CancellationToken ct)
    {
        var normalizedPermissionCode = permissionCode.Trim();
        var isPublished = modules
            .SelectMany(module => module.Permissions)
            .Any(permission => string.Equals(permission.Code, normalizedPermissionCode, StringComparison.Ordinal));

        if (!isPublished)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["permissions"] = [$"Permission '{normalizedPermissionCode}' is not published by any registered module."]
            });
        }

        var role = await dbContext.Roles
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (role is null)
            throw new NotFoundException($"Role with ID {id} was not found");

        role.AddPermission(normalizedPermissionCode);
    }
}