using BuildingBlocks.Modules.Users.Domain.Aggregates;
using BuildingBlocks.Modules.Users.Domain.Enums;
using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserByExternalIdentity;

/// <summary>
/// Query to retrieve a user by their external identity (provider + external user ID).
/// Used by middleware and inter-module communication.
/// </summary>
public record GetUserByExternalIdentityQuery(
    IdentityProvider Provider,
    string ExternalUserId) : IRequest<User?>;
