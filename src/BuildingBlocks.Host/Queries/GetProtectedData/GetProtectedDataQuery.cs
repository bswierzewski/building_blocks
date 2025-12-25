using BuildingBlocks.Abstractions.Attributes;
using MediatR;

namespace BuildingBlocks.Host.Queries.GetProtectedData;

[Authorize(Roles = [HostModule.Roles.Admin])]
public record GetProtectedDataQuery(string UserId) : IRequest<GetProtectedDataResponse>;

public record GetProtectedDataResponse(
    string Message,
    string UserId,
    DateTime Timestamp);
