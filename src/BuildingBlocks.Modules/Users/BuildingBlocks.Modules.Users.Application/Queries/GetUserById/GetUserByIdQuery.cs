using BuildingBlocks.Modules.Users.Domain.Aggregates;
using MediatR;

namespace BuildingBlocks.Modules.Users.Application.Queries.GetUserById;

/// <summary>
/// Query to retrieve a user by their internal ID.
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<User?>;
