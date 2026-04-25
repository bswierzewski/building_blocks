namespace BuildingBlocks.Identity.Features.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string Description,
    string[]? Permissions);

public sealed record CreateRoleResponse(Guid Id);