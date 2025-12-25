using MediatR;

namespace BuildingBlocks.Host.Queries.GetHealth;

public record GetHealthQuery : IRequest<GetHealthResponse>;

public record GetHealthResponse(string Status, DateTime Timestamp);
