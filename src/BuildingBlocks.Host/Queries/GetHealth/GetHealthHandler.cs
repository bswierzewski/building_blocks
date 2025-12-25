using MediatR;

namespace BuildingBlocks.Host.Queries.GetHealth;

public class GetHealthHandler : IRequestHandler<GetHealthQuery, GetHealthResponse>
{
    public Task<GetHealthResponse> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var response = new GetHealthResponse(
            Status: "Healthy",
            Timestamp: DateTime.UtcNow
        );

        return Task.FromResult(response);
    }
}
