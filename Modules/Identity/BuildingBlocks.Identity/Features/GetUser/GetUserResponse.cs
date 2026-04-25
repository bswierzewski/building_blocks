using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Features.GetUser;

public sealed record GetUserResponse(
    Guid Id,
    string Email,
    UserStatus Status,
    IReadOnlyCollection<GetUserRoleResponse> Roles,
    IReadOnlyCollection<GetUserExternalAccountResponse> ExternalAccounts);

public sealed record GetUserRoleResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<string> Permissions);

public sealed record GetUserExternalAccountResponse(
    Guid Id,
    ExternalProvider Provider,
    string ExternalId);