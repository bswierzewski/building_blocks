using BuildingBlocks.Tests.Core;

namespace BuildingBlocks.Tests.Fixtures;

public interface ITestHostFixture
{
    TestContext Context { get; }
}
