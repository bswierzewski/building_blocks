namespace BuildingBlocks.Identity.Features.GetRoles;

public sealed record GetRolesResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<string> Permissions,
    int UserCount);