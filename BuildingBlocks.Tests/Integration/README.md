# Integration Test Infrastructure

This folder contains the shared infrastructure for application-level integration tests built on Alba, xUnit, Testcontainers, and Respawn.

The design has two main concepts:

1. `IntegrationTestCollection<TProgram>`
   The xUnit fixture for one shared test stack. Starts the PostgreSQL container and handles database resets. Override `ConfigureServices` to register collection-wide service replacements (e.g. a fake HTTP client used by every test in the collection).

2. `IntegrationTestBase<TProgram>`
   Base class for individual test classes. For every test it automatically:
   - resets the database
   - creates the Alba host (applying collection-wide and per-class service overrides)
   - runs per-class seed logic
   - disposes the host after the test

## Recommended Usage Model

- Use one collection for one shared application stack.
- Mark the collection with `DisableParallelization = true`.
- Collection-wide service overrides go in `IntegrationTestCollection.ConfigureServices`.
- Per-class overrides go in `IntegrationTestBase.ConfigureServices`.
- Per-class seed data goes in `SeedDataAsync`.

## Create A Collection

```csharp
using BuildingBlocks.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MyApp.IntegrationTests.Shared;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SharedCollection : IntegrationTestCollection<Program>, ICollectionFixture<SharedCollection>
{
    public const string Name = "Shared";

    // Optional: collection-wide service replacements
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEmailSender, FakeEmailSender>();
    }
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
    SharedCollection collection,
    ITestOutputHelper output)
    : IntegrationTestBase<Program>(collection)
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

Create a separate collection when you need a different shared stack, for example:

- different collection-wide service registrations
- a different connection string configuration key
- additional in-memory configuration entries

Each collection gets its own PostgreSQL container.