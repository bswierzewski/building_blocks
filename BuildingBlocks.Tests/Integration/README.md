# Integration Test Infrastructure

This folder contains the shared infrastructure for application-level integration tests built on Alba, xUnit, Testcontainers, and Respawn.

The design has three main concepts:

1. `IntegrationTestEnvironment<TProgram>`
   Owns the shared test environment for one xUnit collection. It starts the PostgreSQL container, exposes the connection string, applies environment variables during host creation, and creates Alba hosts for tests.

2. `IntegrationTestCollection<TEnvironment>`
   Connects an xUnit collection to a single shared test environment. One collection should represent one shared application stack.

3. `IntegrationTestBase<TProgram>`
   Provides the base lifecycle for individual test classes. For every test it automatically:
   - resets the database before the host is created
   - creates the Alba host
   - applies class-level service overrides
   - runs class-level seed logic
   - disposes the host after the test
   - resets the database again after the test

## Recommended Usage Model

- Use one collection for one shared stack.
- Mark the collection with `DisableParallelization = true`.
- If you need different environment variables or a different connection string, create a new environment class and a new collection.
- Class-specific overrides belong in `ConfigureServices`.
- Class-specific seed data belongs in `SeedDataAsync`.

## Create A Shared Environment

```csharp
using BuildingBlocks.Tests.Integration;

namespace MyApp.IntegrationTests.Shared;

public sealed class SharedEnvironment : IntegrationTestEnvironment<Program>
{
    protected override string EnvironmentName => "Testing";

    protected override IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings__Default"] = ConnectionString,
            ["FeatureFlags__UseSandbox"] = "true"
        };
    }
}
```

## Create A Shared Collection

```csharp
using BuildingBlocks.Tests.Integration;
using Xunit;

namespace MyApp.IntegrationTests.Shared;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SharedCollection : IntegrationTestCollection<SharedEnvironment>
{
    public const string Name = "Shared";
}
```

## Example Test Class

```csharp
using BuildingBlocks.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace MyApp.IntegrationTests.Features.Orders;

[Collection(SharedCollection.Name)]
public sealed class GenerateDeliveryNoteTests(
    SharedEnvironment testEnvironment,
    ITestOutputHelper output)
    : IntegrationTestBase<Program>(testEnvironment)
{
    private readonly Mock<IProductsClient> _productsClientMock = new();

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IProductsClient>();
        services.AddSingleton(_productsClientMock.Object);
    }

    protected override async Task SeedDataAsync()
    {
        _productsClientMock
            .Setup(x => x.GetProductsAsync())
            .ReturnsAsync(new Dictionary<string, Product>());

        using var scope = AlbaHost.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        dbContext.Entities.Add(new MyEntity());
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task should_generate_document()
    {
        var result = await AlbaHost.Scenario(s =>
        {
            s.Post.Json(new { Id = Guid.NewGuid() }).ToUrl("/documents");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        output.WriteLine($"Response content type: {result.Context.Response.ContentType}");
    }
}
```

## What Changes Per Collection

Create a separate environment and a separate collection when you need a different shared stack, for example:

- a different connection string variable name
- additional environment variables
- a different ASP.NET Core environment name
- different environment-level service registration in `ConfigureEnvironmentServices`
- different web host customization in `ConfigureHost`

Each collection gets its own `IntegrationTestEnvironment`, and that means its own PostgreSQL container.