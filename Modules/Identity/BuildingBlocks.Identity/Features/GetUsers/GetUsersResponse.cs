using BuildingBlocks.Identity.Domain.Enums;

namespace BuildingBlocks.Identity.Features.GetUsers;

public sealed record GetUsersResponse(
    Guid Id,
    string Email,
    UserStatus Status,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<GetUsersExternalAccountResponse> ExternalAccounts);

public sealed record GetUsersExternalAccountResponse(
    Guid Id,
    ExternalProvider Provider,
    string ExternalId);