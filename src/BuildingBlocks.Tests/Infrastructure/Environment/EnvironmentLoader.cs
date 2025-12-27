using DotNetEnv;
using System.Runtime.CompilerServices;

namespace BuildingBlocks.Tests.Infrastructure.Environment;

internal static class EnvironmentLoader
{
    [ModuleInitializer]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2255:The ModuleInitializer attribute should not be used in libraries")]
    public static void Initialize()
    {
        Console.WriteLine($"[{nameof(EnvironmentLoader)}] Current Directory: {Directory.GetCurrentDirectory()}");

        var envPath = Env.TraversePath().Load();

        Console.WriteLine($"[{nameof(EnvironmentLoader)}] .env found and loaded (count: {envPath.Count()})");
    }
}
