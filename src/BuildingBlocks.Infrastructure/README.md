# BuildingBlocks.Infrastructure

A comprehensive .NET infrastructure layer library providing essential building blocks for Clean Architecture applications with Entity Framework Core, automatic migrations, audit interceptors, and domain event dispatching.

## 📦 Installation

```bash
dotnet add package BuildingBlocks.Infrastructure
```

## 🚀 Quick Start

### 1. Register Basic Infrastructure Services

```csharp
using BuildingBlocks.Infrastructure;

// In your Program.cs
builder.Services.AddBuildingBlocksInfrastructure();
```

### 2. Register Module with DbContext

```csharp
// Full module (all features enabled by default)
services.AddModule<OrdersDbContext>();

// Or with custom configuration
services.AddModule<OrdersDbContext>(
    configureApplication: app => app.DisableValidation(),
    configureInfrastructure: infra => infra.DisableMigrations()
);
```

### 3. Configure Your DbContext

```csharp
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Your entity configurations
    }
}
```

## 🔧 Features

### Opt-Out Configuration Approach

All infrastructure features are **enabled by default**. Use `DisableX()` methods to turn off specific features:

- **Migrations** - Automatic database migrations on startup
- **Auditable Interceptor** - Automatic CreatedAt/CreatedBy/ModifiedAt/ModifiedBy population
- **Domain Event Dispatch** - Automatic domain event publishing after SaveChanges

### Infrastructure Components

#### 1. Migration Service
Automatically applies pending migrations on application startup with detailed logging:

```csharp
// Enabled by default, disable with:
services.AddModule<OrdersDbContext>(
    configureInfrastructure: infra => infra.DisableMigrations()
);
```

#### 2. Auditable Entity Interceptor
Automatically populates audit fields for entities implementing `IAuditable`:

```csharp
public class Order : IAuditable
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Auto-populated by interceptor
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

// Disable with:
services.AddModule<OrdersDbContext>(
    configureInfrastructure: infra => infra.DisableAuditableInterceptor()
);
```

#### 3. Domain Event Dispatch Interceptor
Automatically publishes domain events after successful SaveChanges:

```csharp
public class Order : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Disable with:
services.AddModule<OrdersDbContext>(
    configureInfrastructure: infra => infra.DisableDomainEventDispatch()
);
```

## 📋 Configuration Options

### ModuleInfrastructureBuilder Methods

#### Disable Methods
- `DisableMigrations()` - Turn off automatic migrations
- `DisableAuditableInterceptor()` - Turn off audit field population
- `DisableDomainEventDispatch()` - Turn off domain event publishing

#### Predefined Setups
- `DisableAllInterceptors()` - Disable both audit and domain event interceptors
- `UseMinimalSetup()` - Keep only auditable interceptor (disable migrations and domain events)
- `UseReadOnlySetup()` - Disable all features (for read-only scenarios)
- `UseMigrationOnlySetup()` - Keep only migrations (disable all interceptors)

### Usage Examples

```csharp
// Full module (default - everything enabled)
services.AddModule<OrdersDbContext>();

// Minimal setup (audit only)
services.AddModule<OrdersDbContext>(
    configureInfrastructure: infra => infra.UseMinimalSetup()
);

// Read-only module
services.AddModule<ReportsDbContext>(
    configureInfrastructure: infra => infra.UseReadOnlySetup()
);

// Custom configuration
services.AddModule<OrdersDbContext>(
    configureInfrastructure: infra => infra
        .DisableMigrations()
        .DisableDomainEventDispatch()
);

// Combined with application configuration
services.AddModule<OrdersDbContext>(
    configureApplication: app => app.DisableValidation(),
    configureInfrastructure: infra => infra.DisableMigrations()
);
```

## 🏗️ Architecture Integration

This library serves as the Infrastructure layer in Clean Architecture:

```
┌─────────────────────────────────────┐
│           Presentation              │
│        (Web API, MVC, etc.)         │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│     BuildingBlocks.Application      │
│     (Use Cases, Behaviors)          │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│   BuildingBlocks.Infrastructure     │ ◄── This Package
│   (Data Access, External Services)  │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│           Domain                    │
│    (Entities, Value Objects)        │
└─────────────────────────────────────┘
```

## 🔄 Dependencies

This package depends on:

- **Microsoft.EntityFrameworkCore (9.0.0)** - Entity Framework Core
- **Microsoft.Extensions.DependencyInjection.Abstractions (9.0.0)** - Dependency injection
- **Microsoft.Extensions.Hosting.Abstractions (9.0.0)** - For hosted services
- **Microsoft.Extensions.Logging.Abstractions (9.0.0)** - Logging abstractions
- **MediatR (12.4.0)** - For domain event dispatching
- **FluentValidation (12.0.0)** - For validation behaviors
- **BuildingBlocks.Application** - Application layer abstractions
- **BuildingBlocks.Domain** - Domain entities and interfaces

## 📚 Advanced Examples

### Custom DbContext with Interceptors

```csharp
public class OrdersDbContext : DbContext
{
    private readonly AuditableEntityInterceptor _auditInterceptor;
    private readonly DispatchDomainEventsInterceptor _domainEventInterceptor;
    
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        AuditableEntityInterceptor auditInterceptor,
        DispatchDomainEventsInterceptor domainEventInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
        _domainEventInterceptor = domainEventInterceptor;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Interceptors are automatically registered if enabled
        optionsBuilder.AddInterceptors(_auditInterceptor, _domainEventInterceptor);
    }
    
    public DbSet<Order> Orders { get; set; }
}
```

### Modular Registration

```csharp
// Register multiple modules with different configurations
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        return services.AddModule<OrdersDbContext>(
            configureApplication: app => app
                .DisableAuthorization(), // Orders don't need auth
            configureInfrastructure: infra => infra
                .DisableDomainEventDispatch() // No domain events for orders
        );
    }
    
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        return services.AddModule<ReportsDbContext>(
            configureApplication: app => app
                .UseReadOnlySetup(), // Read-only queries
            configureInfrastructure: infra => infra
                .UseReadOnlySetup() // No writes needed
        );
    }
}
```

## 🤝 Contributing

Please ensure that any contributions:
1. Follow the existing patterns and conventions
2. Include appropriate XML documentation
3. Add unit tests for new functionality
4. Maintain backward compatibility
5. Use the opt-out configuration approach for new features

## 📄 License

[Add your license information here]
