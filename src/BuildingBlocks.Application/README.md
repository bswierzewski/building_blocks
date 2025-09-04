# BuildingBlocks.Application

A comprehensive .NET application layer library providing essential building blocks for Clean Architecture applications with MediatR, FluentValidation, and common behavioral patterns.

## 📦 Installation

```bash
dotnet add package BuildingBlocks.Application
```

## 🚀 Quick Start

### 1. Register Services

```csharp
using BuildingBlocks.Application;

// In your Program.cs
builder.Services.AddApplicationServices();
```

### 2. Implement a Request Handler

```csharp
public class CreateUserCommand : IRequest<Result<int>>
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        // Logging, validation, authorization, and performance monitoring
        // are handled automatically by the pipeline behaviors
        
        return Result<int>.Success(userId);
    }
}
```

### 3. Add Validation

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

## 🔧 Features

### Pipeline Behaviors

The library automatically registers the following MediatR pipeline behaviors in order:

1. **LoggingBehavior** - Comprehensive request/response logging with timing
2. **UnhandledExceptionBehavior** - Global exception handling and logging
3. **AuthorizationBehavior** - Request-level authorization using `[Authorize]` attributes
4. **ValidationBehavior** - Automatic FluentValidation integration
5. **PerformanceBehavior** - Performance monitoring and slow request detection

### Core Components

#### Result Pattern
```csharp
// Success result
var result = Result<User>.Success(user);

// Error result  
var result = Result<User>.Failure("User not found");

// Check result
if (result.IsSuccess)
{
    var user = result.Value;
}
else
{
    var error = result.Error;
}
```

#### Paginated Lists
```csharp
var query = context.Users.AsQueryable();
var paginatedUsers = await query.ToPaginatedListAsync(pageNumber: 1, pageSize: 10);

// Returns PaginatedList<User> with:
// - Items: List<User>
// - PageNumber: int
// - TotalPages: int  
// - TotalCount: int
// - HasPreviousPage: bool
// - HasNextPage: bool
```

#### User Abstraction
```csharp
public class MyHandler : IRequestHandler<MyCommand>
{
    private readonly IUser _user;
    
    public MyHandler(IUser user)
    {
        _user = user;
    }
    
    public async Task Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        var isAuthenticated = _user.IsAuthenticated;
        var isInRole = _user.IsInRole("Admin");
    }
}
```

### Authorization

Apply authorization to your requests using the `[Authorize]` attribute:

```csharp
[Authorize] // Requires authentication
public class DeleteUserCommand : IRequest<Result>
{
    public int UserId { get; set; }
}

[Authorize(Roles = "Admin")] // Requires specific role
public class ManageUsersCommand : IRequest<Result>
{
    // Command properties
}

[Authorize(Policy = "CanEditUsers")] // Requires specific policy
public class EditUserCommand : IRequest<Result>
{
    // Command properties  
}
```

### Exception Handling

Built-in exceptions for common scenarios:

```csharp
// For authorization failures
throw new ForbiddenAccessException();

// For validation failures (automatically thrown by ValidationBehavior)
throw new ValidationException(validationFailures);
```

### Extensions

Useful extension methods for common operations:

```csharp
// Enumerable extensions
var distinctBy = items.DistinctBy(x => x.Id);
var chunked = items.Chunk(10);

// Queryable extensions  
var paginatedResult = await query.ToPaginatedListAsync(1, 10, cancellationToken);
```

## 📋 Configuration

### Logging Configuration

The `LoggingBehavior` provides detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "BuildingBlocks.Application.Behaviors.LoggingBehavior": "Debug"
    }
  }
}
```

### Performance Monitoring

Configure performance thresholds in your application settings:

```csharp
// Custom performance behavior configuration
services.Configure<PerformanceOptions>(options =>
{
    options.SlowRequestThreshold = TimeSpan.FromSeconds(2);
});
```

## 🔄 Dependencies

This package depends on:

- **MediatR (12.4.0)** - CQRS and mediator pattern
- **FluentValidation (12.0.0)** - Input validation
- **FluentValidation.DependencyInjectionExtensions (12.0.0)** - DI integration
- **Microsoft.Extensions.DependencyInjection.Abstractions (9.0.0)** - Dependency injection
- **Microsoft.Extensions.Logging.Abstractions (9.0.0)** - Logging abstractions
- **Microsoft.EntityFrameworkCore (9.0.0)** - For pagination extensions

## 🏗️ Architecture Integration

This library is designed to work as the Application layer in Clean Architecture:

```
┌─────────────────────────────────────┐
│           Presentation              │ 
│        (Web API, MVC, etc.)         │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│     BuildingBlocks.Application      │ ◄── This Package
│     (Use Cases, Behaviors)          │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│           Domain                    │
│    (Entities, Value Objects)        │
└─────────────────────────────────────┘
```

## 📚 Examples

### Complete CRUD Example

```csharp
// Command
public class UpdateUserCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// Validator
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

// Handler
[Authorize]
public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IUserRepository _repository;
    private readonly IUser _currentUser;
    
    public UpdateUserHandler(IUserRepository repository, IUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }
    
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return Result.Failure("User not found");
            
        // Check if current user can edit this user
        if (!_currentUser.IsInRole("Admin") && _currentUser.Id != user.Id)
            throw new ForbiddenAccessException();
            
        user.UpdateDetails(request.Email, request.Name);
        await _repository.UpdateAsync(user, cancellationToken);
        
        return Result.Success();
    }
}
```

## 🤝 Contributing

Please ensure that any contributions:
1. Follow the existing patterns and conventions
2. Include appropriate XML documentation
3. Add unit tests for new functionality
4. Maintain backward compatibility

## 📄 License

[Add your license information here]