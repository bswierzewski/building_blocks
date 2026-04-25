using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Features.GetRole;

public sealed record GetRoleResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<GetRoleUserResponse> Users);

public sealed record GetRoleUserResponse(
    Guid Id,
    string Email,
    UserStatus Status);