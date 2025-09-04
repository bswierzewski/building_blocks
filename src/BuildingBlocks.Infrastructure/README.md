# BuildingBlocks.Infrastructure

A comprehensive .NET infrastructure layer library providing implementations for external concerns like data access, caching, messaging, and third-party integrations for Clean Architecture applications.

## 📦 Installation

```bash
dotnet add package BuildingBlocks.Infrastructure
```

## 🚀 Quick Start

### 1. Register Infrastructure Services

```csharp
using BuildingBlocks.Infrastructure;

// In your Program.cs
builder.Services.AddInfrastructureServices(builder.Configuration);
```

### 2. Entity Framework Integration

```csharp
using BuildingBlocks.Infrastructure.Data;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Your entity configurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### 3. Repository Implementation

```csharp
using BuildingBlocks.Infrastructure.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
    
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);
    }
}
```

## 🔧 Features

### Data Access

#### BaseDbContext
Enhanced DbContext with domain events and audit support:
```csharp
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options) { }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatic audit fields update
        await UpdateAuditFieldsAsync();
        
        // Dispatch domain events
        await DispatchDomainEventsAsync();
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

#### Generic Repository
Base repository implementation:
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;
    
    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }
    
    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }
    
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
    
    // Additional repository methods...
}
```

### Specifications Support

```csharp
public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, ISpecification<T> spec)
        where T : BaseEntity
    {
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
            
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
            
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);
            
        return query;
    }
}
```

### Caching

#### Distributed Cache Integration
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistributedCacheService> _logger;
    
    // Implementation with JSON serialization and logging
}
```

### Messaging

#### Event Bus Integration
```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : BaseEvent;
}

// Example implementations for different message brokers
public class InMemoryEventBus : IEventBus { /* Implementation */ }
public class ServiceBusEventBus : IEventBus { /* Azure Service Bus implementation */ }
public class RabbitMqEventBus : IEventBus { /* RabbitMQ implementation */ }
```

### External Services

#### Email Service
```csharp
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public class SmtpEmailService : IEmailService
{
    // SMTP implementation
}

public class SendGridEmailService : IEmailService
{
    // SendGrid implementation
}
```

#### File Storage
```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string fileId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileId, CancellationToken cancellationToken = default);
}

public class LocalFileStorageService : IFileStorageService { /* Local storage */ }
public class AzureBlobStorageService : IFileStorageService { /* Azure Blob Storage */ }
public class S3FileStorageService : IFileStorageService { /* Amazon S3 */ }
```

### Background Services

#### Domain Event Processing
```csharp
public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public async Task PublishAsync(BaseEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing domain event: {EventType}", domainEvent.GetType().Name);
        
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);
        
        var tasks = handlers.Select(handler => 
            ((IDomainEventHandler<BaseEvent>)handler).Handle(domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
    }
}
```

### Health Checks

```csharp
public static class HealthCheckExtensions
{
    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddRedis(configuration.GetConnectionString("Redis"))
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .AddCheck<EmailServiceHealthCheck>("email")
            .AddCheck<StorageServiceHealthCheck>("storage");
            
        return services;
    }
}
```

## ⚙️ Configuration

### Database Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApp;Trusted_Connection=true;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  },
  "Email": {
    "Provider": "SMTP", // or "SendGrid"
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-password"
    }
  },
  "Storage": {
    "Provider": "Local", // or "AzureBlob", "S3"
    "Local": {
      "BasePath": "./uploads"
    }
  }
}
```

### Service Registration

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Caching
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis"));
        
        // Services
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<IDomainEventService, DomainEventService>();
        
        // External services based on configuration
        AddEmailService(services, configuration);
        AddStorageService(services, configuration);
        
        return services;
    }
}
```

## 🏗️ Architecture Integration

This library implements the Infrastructure layer in Clean Architecture:

```
┌─────────────────────────────────────┐
│           Presentation              │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│          Application                │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│  BuildingBlocks.Infrastructure      │ ◄── This Package
│   (Data Access, External Services)  │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│           Domain                    │
└─────────────────────────────────────┘
```

## 📚 Examples

### Complete Infrastructure Setup

```csharp
// Startup/Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add infrastructure services
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddInfrastructureHealthChecks(builder.Configuration);
        
        var app = builder.Build();
        
        // Use health checks
        app.MapHealthChecks("/health");
        
        app.Run();
    }
}

// Entity Configuration
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}

// Domain Event Handler
public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCreatedEventHandler> _logger;
    
    public UserCreatedEventHandler(IEmailService emailService, ILogger<UserCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending welcome email to user {UserId}", domainEvent.UserId);
        
        await _emailService.SendAsync(
            domainEvent.Email,
            "Welcome!",
            "Welcome to our application!",
            cancellationToken);
    }
}
```

## 🔄 Dependencies

This package depends on:

- **BuildingBlocks.Domain** - Domain layer abstractions
- **BuildingBlocks.Application** - Application layer interfaces
- **Microsoft.EntityFrameworkCore** - Entity Framework Core
- **Microsoft.Extensions.Caching.StackExchangeRedis** - Redis caching
- **Microsoft.Extensions.Diagnostics.HealthChecks** - Health checks
- And other infrastructure-specific packages

## 🤝 Contributing

Please ensure that any contributions:
1. Follow Clean Architecture principles
2. Include appropriate integration tests
3. Add XML documentation for public APIs
4. Follow the established patterns for service registration

## 📄 License

MIT