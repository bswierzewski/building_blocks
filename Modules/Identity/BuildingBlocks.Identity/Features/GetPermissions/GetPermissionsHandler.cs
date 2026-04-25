using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;

namespace BuildingBlocks.Identity.Features.GetPermissions;

public sealed class GetPermissionsHandler
{
    [WolverineGet("/api/identity/permissions")]
    [Tags("Identity")]
    [EndpointName("GetPermissions")]
    [EndpointSummary("Get all published permissions")]
    public static List<GetPermissionsResponse> Handle(IEnumerable<IModule> modules)
    {
        return modules
            .SelectMany(module => module.Permissions, (module, permission) => new GetPermissionsResponse(
                module.Name,
                permission.Code,
                permission.Description))
            .DistinctBy(permission => permission.Code, StringComparer.Ordinal)
            .OrderBy(permission => permission.Module, StringComparer.Ordinal)
            .ThenBy(permission => permission.Code, StringComparer.Ordinal)
            .ToList();
    }
}