namespace BuildingBlocks.Identity.Features.GetPermissions;

public sealed record GetPermissionsResponse(
    string Module,
    string Code,
    string Description);