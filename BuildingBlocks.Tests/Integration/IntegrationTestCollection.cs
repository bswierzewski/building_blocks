using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base type for a single xUnit collection definition per integration-test environment.
/// Apply CollectionDefinition with DisableParallelization = true on a derived non-generic class.
/// </summary>
public abstract class IntegrationTestCollection<TEnvironment> : ICollectionFixture<TEnvironment>
    where TEnvironment : class, IAsyncLifetime
{
}