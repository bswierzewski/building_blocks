using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.Queries.BuildInfo;

/// <summary>
/// Handles retrieval of application build information by processing GetBuildInfoQuery requests.
/// </summary>
public class GetBuildInfoHandler : IRequestHandler<GetBuildInfoQuery, Result<BuildInfoDto>>
{
    /// <summary>
    /// Retrieves build information from environment variables.
    /// </summary>
    /// <param name="request">Query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Build information including commit SHA, build date, and environment.</returns>
    public Task<Result<BuildInfoDto>> Handle(GetBuildInfoQuery request, CancellationToken cancellationToken)
    {
        var commitSha = Environment.GetEnvironmentVariable("GIT_COMMIT_SHA") ?? "unknown";
        var buildDate = Environment.GetEnvironmentVariable("BUILD_DATE") ?? "unknown";

        // Read environment name from environment variables (works for both ASPNETCORE_ENVIRONMENT and DOTNET_ENVIRONMENT)
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                           ?? "Production";

        var buildInfo = new BuildInfoDto(
            CommitSha: commitSha,
            BuildDate: buildDate,
            Environment: environmentName
        );

        return Task.FromResult(Result<BuildInfoDto>.Success(buildInfo));
    }
}
