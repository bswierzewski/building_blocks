# Integration Test Infrastructure

This folder contains the shared infrastructure for application-level integration tests built on Alba, xUnit, Testcontainers, and Respawn.

The design has three main concepts:

1. `IntegrationTestEnvironment<TProgram>`
    The shared runtime environment for one integration-test stack. Starts the PostgreSQL container, initializes the schema, handles database resets, and exposes environment-wide service overrides.

2. `IntegrationTestCollection<TEnvironment>`
    Connects one xUnit collection to one shared integration-test environment.

3. `IntegrationTestBase<TProgram>`
   Base class for individual test classes. For every test it automatically:
   - resets the database
    - creates the Alba host (applying environment-wide and per-class service overrides)
   - runs per-class seed logic
   - disposes the host after the test

## Recommended Usage Model

- Use one collection for one shared integration-test environment.
- Mark the collection with `DisableParallelization = true`.
- Environment-wide service overrides go in `IntegrationTestEnvironment.ConfigureServices`.
- Per-class overrides go in `IntegrationTestBase.ConfigureServices`.
- Per-class seed data goes in `SeedDataAsync`.

## Create An Environment

```csharp
using BuildingBlocks.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MyApp.IntegrationTests.Shared;

public sealed class SharedEnvironment : IntegrationTestEnvironment<Program>
{
    // Optional: environment-wide service replacements
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEmailSender, FakeEmailSender>();
    }

    protected override async ValueTask InitializeDatabaseAsync()
    {
        await using var dbContext = new MyDbContext(
            new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(ConnectionString)
                .Options);

        await dbContext.Database.MigrateAsync();
    }
}
```

## Create A Collection

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
    SharedEnvironment environment,
    ITestOutputHelper output)
    : IntegrationTestBase<Program>(environment)
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

- different environment-wide service registrations
- a different connection string configuration key
- additional in-memory configuration entries

Each environment gets its own PostgreSQL container.