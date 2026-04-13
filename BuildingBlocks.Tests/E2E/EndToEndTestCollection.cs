using Xunit;

namespace BuildingBlocks.Tests.E2E;

/// <summary>
/// Base type for a single xUnit collection definition per distributed application environment.
/// Apply CollectionDefinition with DisableParallelization = true on a derived non-generic class.
/// </summary>
public abstract class EndToEndTestCollection<TEnvironment> : ICollectionFixture<TEnvironment>
    where TEnvironment : class, IAsyncLifetime
{
}