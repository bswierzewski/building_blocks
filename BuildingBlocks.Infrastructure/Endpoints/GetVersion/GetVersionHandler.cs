using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine.Http;

namespace BuildingBlocks.Infrastructure.Endpoints.GetVersion;

public static class GetVersionHandler
{
    [WolverineGet("/api/system/version")]
    [Tags("System")]
    [EndpointName("GetVersion")]
    [EndpointSummary("Get API version and build information")]
    public static GetVersionResponse Handle()
    {
        var entry = Assembly.GetEntryAssembly();

        var infoVersion = entry?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";

        // dotnet publish -p:SourceRevisionId=<sha> produces "1.0.0+<sha>"
        var sha = infoVersion.Contains('+')
            ? infoVersion[(infoVersion.LastIndexOf('+') + 1)..]
            : null;

        return new GetVersionResponse(sha);
    }
}
