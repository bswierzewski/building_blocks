# End-To-End Test Infrastructure

This folder contains the shared infrastructure for Aspire end-to-end tests.

The design mirrors the Integration folder, but the runtime model is different:

- one `EndToEndTestEnvironment<TAppHost>` starts one full `DistributedApplication`
- that distributed application is shared by all tests in one xUnit collection
- tests run as closed-box tests against real resources and processes
- application services cannot be overridden from the test project

## Main Types

1. `EndToEndTestEnvironment<TAppHost>`
   Creates and starts the Aspire AppHost through `DistributedApplicationTestingBuilder`.
   This environment is shared per collection.

2. `EndToEndTestCollection<TEnvironment>`
   Connects one xUnit collection to one shared Aspire environment.

3. `EndToEndTestBase<TAppHost>`
   Gives test classes simple access to helper methods like `CreateHttpClient`, `WaitForResourceHealthyAsync`, and `GetConnectionStringAsync`.

## Recommended Rules

- Use one collection for one distributed application environment.
- Mark the collection with `DisableParallelization = true`.
- Create a new environment and collection only when you need a different AppHost configuration.
- Do not expect DI overrides inside the tested application. Aspire tests are closed-box by design.

## Example Environment

```csharp
using BuildingBlocks.Tests.E2E;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp.E2ETests.Shared;

public sealed class SharedEnvironment : EndToEndTestEnvironment<Projects.MyApp_AppHost>
{
    protected override string[] GetAppHostArguments()
    {
        return [
            "--environment=Testing",
            "DcpPublisher:RandomizePorts=false"
        ];
    }

    protected override Task ConfigureTestingServicesAsync(IServiceCollection services)
    {
        services.AddLogging(logging => logging
            .AddConsole()
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning));

        return Task.CompletedTask;
    }
}
```

## Example Collection

```csharp
using BuildingBlocks.Tests.E2E;
using Xunit;

namespace MyApp.E2ETests.Shared;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SharedCollection : EndToEndTestCollection<SharedEnvironment>
{
    public const string Name = "Shared";
}
```

## Example Test

```csharp
using BuildingBlocks.Tests.E2E;
using System.Net;
using Xunit;

namespace MyApp.E2ETests.Features;

[Collection(SharedCollection.Name)]
public sealed class HomePageTests(SharedEnvironment testEnvironment)
    : EndToEndTestBase<Projects.MyApp_AppHost>(testEnvironment)
{
    [Fact]
    public async Task home_page_returns_ok()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await WaitForResourceHealthyAsync("webfrontend", cts.Token);

        using var client = CreateHttpClient("webfrontend");
        using var response = await client.GetAsync("/", cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## What Is Shared Between Tests

Within a single collection, tests share:

- the same `DistributedApplication`
- the same containers and dependent resources
- the same AppHost process

They do not get per-test DI overrides, and they should be treated as closed-box tests.

If you create a second collection with a second environment, you start a second distributed application stack.