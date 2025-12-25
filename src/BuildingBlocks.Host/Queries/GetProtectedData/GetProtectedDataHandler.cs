using MediatR;

namespace BuildingBlocks.Host.Queries.GetProtectedData;

public class GetProtectedDataHandler : IRequestHandler<GetProtectedDataQuery, GetProtectedDataResponse>
{
    public Task<GetProtectedDataResponse> Handle(GetProtectedDataQuery request, CancellationToken cancellationToken)
    {
        var response = new GetProtectedDataResponse(
            Message: "This is a protected endpoint",
            UserId: request.UserId,
            Timestamp: DateTime.UtcNow
        );

        return Task.FromResult(response);
    }
}
