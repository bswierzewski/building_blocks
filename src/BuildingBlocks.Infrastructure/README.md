# BuildingBlocks.Infrastructure

A comprehensive .NET infrastructure layer library providing essential building blocks for Clean Architecture applications with Entity Framework Core, automatic migrations, audit interceptors, and domain event dispatching.

## 📦 Installation

```bash
dotnet add package BuildingBlocks.Infrastructure
```

## 🚀 Quick Start

### 1. Register Infrastructure Services Individually

```csharp
using BuildingBlocks.Infrastructure;

// Register infrastructure services (choose what you need)
builder.Services
    .AddMigrationService<OrdersDbContext>()    // Auto-migrations
    .AddAuditableEntityInterceptor()           // Audit fields
    .AddDomainEventDispatchInterceptor();      // Domain events
```

### 2. Configure DbContext with Interceptors

```csharp
// Configure DbContext to use registered interceptors
builder.Services.AddDbContext<OrdersDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString)
           .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    // All registered ISaveChangesInterceptor services are automatically included
});
```

### 3. Complete Module Example

```csharp
public static class OrdersModuleExtensions
{
    public static void AddOrdersModule(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("OrdersDb");
        
        // Infrastructure services
        builder.Services
            .AddMigrationService<OrdersDbContext>()    // Automatic migrations
            .AddAuditableEntityInterceptor()           // CreatedAt, ModifiedAt fields
            .AddDomainEventDispatchInterceptor();      // Domain event publishing
            
        // DbContext with interceptors
        builder.Services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                   .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
    }
}
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

### Individual Service Registration

Choose exactly what you need for each module using individual extension methods:

#### ServiceCollectionExtensions
- **`AddMigrationService<TContext>()`** - Registers automatic migration service for the specified DbContext
- **`AddAuditableEntityInterceptor()`** - Registers audit interceptor as ISaveChangesInterceptor
- **`AddDomainEventDispatchInterceptor()`** - Registers domain event interceptor as ISaveChangesInterceptor

#### Key Benefits
- **Granular control** - Add only the features you need
- **Standard interfaces** - Interceptors registered as ISaveChangesInterceptor
- **Automatic discovery** - Use `GetServices<ISaveChangesInterceptor>()` to get all registered interceptors

### Infrastructure Components

#### 1. Migration Service
Automatically applies pending migrations on application startup with detailed logging:

```csharp
// Register for specific DbContext
builder.Services.AddMigrationService<OrdersDbContext>();
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

// Register the interceptor
builder.Services.AddAuditableEntityInterceptor();
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

// Register the interceptor
builder.Services.AddDomainEventDispatchInterceptor();
```

## 📋 Configuration Examples

### Flexible Module Configuration

Each module can be configured exactly as needed by choosing which infrastructure services to register:

#### Full Feature Module
```csharp
public static void AddOrdersModule(this IHostApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("OrdersDb");
    
    // All infrastructure features
    builder.Services
        .AddMigrationService<OrdersDbContext>()     // Automatic migrations
        .AddAuditableEntityInterceptor()            // Audit trail
        .AddDomainEventDispatchInterceptor();       // Domain events
        
    builder.Services.AddDbContext<OrdersDbContext>((sp, options) =>
    {
        options.UseSqlServer(connectionString)
               .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    });
}
```

#### Read-Only Module (Reports)
```csharp
public static void AddReportsModule(this IHostApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("ReportsDb");
    
    // No interceptors needed for read-only
    // Only migrations for schema updates
    builder.Services.AddMigrationService<ReportsDbContext>();
        
    builder.Services.AddDbContext<ReportsDbContext>((sp, options) =>
    {
        options.UseSqlServer(connectionString, opts => opts.QuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
               .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    });
}
```

#### Audit-Only Module
```csharp
public static void AddAuditLogModule(this IHostApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("AuditDb");
    
    // Only auditing, no domain events or migrations (managed separately)
    builder.Services.AddAuditableEntityInterceptor();
        
    builder.Services.AddDbContext<AuditDbContext>((sp, options) =>
    {
        options.UseSqlServer(connectionString)
               .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    });
}
```

#### Event-Driven Module
```csharp
public static void AddEventDrivenModule(this IHostApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("EventsDb");
    
    // Focus on domain events, minimal other features
    builder.Services
        .AddMigrationService<EventsDbContext>()
        .AddDomainEventDispatchInterceptor();  // No auditing needed
        
    builder.Services.AddDbContext<EventsDbContext>((sp, options) =>
    {
        options.UseSqlServer(connectionString)
               .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    });
}
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

### Custom DbContext Setup

```csharp
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Your entity configurations
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
```

### Complete Modular Registration Pattern

```csharp
// Extension methods for clean module registration
public static class ServiceCollectionExtensions
{
    public static void AddOrdersModule(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("OrdersDb");
        
        // Application layer
        builder.Services.AddValidators();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterHandlers()
               .AddLoggingBehavior()
               .AddValidationBehavior()
               .AddAuthorizationBehavior();
        });
        
        // Infrastructure layer
        builder.Services
            .AddMigrationService<OrdersDbContext>()
            .AddAuditableEntityInterceptor()
            .AddDomainEventDispatchInterceptor();
            
        // DbContext
        builder.Services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        
        // Domain services
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    }
    
    public static void AddReportsModule(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("ReportsDb");
        
        // Minimal setup for read-only module
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterHandlers()
               .AddLoggingBehavior()
               .AddPerformanceMonitoringBehavior();
        });
        
        builder.Services.AddMigrationService<ReportsDbContext>();
        
        builder.Services.AddDbContext<ReportsDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
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
