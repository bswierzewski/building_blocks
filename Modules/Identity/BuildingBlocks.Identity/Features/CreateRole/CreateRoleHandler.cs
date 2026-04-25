using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Identity.Domain.Aggregates;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.CreateRole;

public sealed class CreateRoleHandler
{
    [WolverinePost("/identity/roles")]
    [Tags("Identity")]
    [EndpointName("CreateRole")]
    [EndpointSummary("Create a new role")]
    public static async Task<CreateRoleResponse> Handle(
        CreateRoleCommand command,
        IdentityDbContext dbContext,
        IEnumerable<IModule> modules,
        CancellationToken ct)
    {
        var roleName = command.Name.Trim();
        var roleDescription = command.Description.Trim();
        var permissionCodes = command.Permissions?
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];

        var publishedPermissions = modules
            .SelectMany(module => module.Permissions)
            .Select(permission => permission.Code)
            .ToHashSet(StringComparer.Ordinal);

        var invalidPermissions = permissionCodes
            .Where(code => !publishedPermissions.Contains(code))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();

        if (invalidPermissions.Length > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["permissions"] = invalidPermissions
                    .Select(code => $"Permission '{code}' is not published by any registered module.")
                    .ToArray()
            });
        }

        var roleExists = await dbContext.Roles
            .AsNoTracking()
            .AnyAsync(role => role.Name == roleName, ct);

        if (roleExists)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = [$"Role '{roleName}' already exists."]
            });
        }

        var role = Role.Create(roleName, roleDescription);

        foreach (var permissionCode in permissionCodes)
            role.AddPermission(permissionCode);

        await dbContext.Roles.AddAsync(role, ct);

        return new CreateRoleResponse(role.Id);
    }
}