using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.Queries.BuildInfo;

/// <summary>
/// Query to retrieve application build information.
/// </summary>
public record GetBuildInfoQuery : IRequest<Result<BuildInfoDto>>;

/// <summary>
/// Data transfer object containing build information.
/// </summary>
/// <param name="CommitSha">Full git commit SHA.</param>
/// <param name="BuildDate">Build date in ISO 8601 format.</param>
/// <param name="Environment">Application environment (Development, Production, etc.).</param>
public record BuildInfoDto(
    string CommitSha,
    string BuildDate,
    string Environment
);
