# BuildingBlocks

A comprehensive .NET 9.0 solution implementing Clean Architecture with reusable building blocks for enterprise applications.

## 📋 Overview

BuildingBlocks provides a solid foundation for building scalable, maintainable applications using Clean Architecture principles. The solution includes common patterns, behaviors, and abstractions that can be reused across multiple projects.

## 🏗️ Architecture

The solution follows Clean Architecture with three main layers:

```
├── Domain/          # Core business entities and domain logic (no dependencies)
├── Application/     # Application services, use cases, and interfaces
└── Infrastructure/  # External concerns (data access, third-party integrations)
```

### Dependency Flow
- **Domain** → No external dependencies
- **Application** → Depends only on Domain
- **Infrastructure** → Depends on both Domain and Application

## 🚀 Features

### Domain Layer (`BuildingBlocks.Domain`)
- Base entity classes with auditing support
- Domain events infrastructure
- Value objects base classes
- Repository patterns
- Specification pattern

### Application Layer (`BuildingBlocks.Application`)
- **MediatR Pipeline Behaviors:**
  - `LoggingBehavior` - Comprehensive request/response logging
  - `ValidationBehavior` - FluentValidation integration
  - `AuthorizationBehavior` - Request authorization
  - `PerformanceBehavior` - Performance monitoring
  - `UnhandledExceptionBehavior` - Global exception handling

- **Common Patterns:**
  - CQRS with MediatR
  - Result pattern for error handling
  - Pagination support
  - User abstractions

### Infrastructure Layer (`BuildingBlocks.Infrastructure`)
- Entity Framework Core integrations
- External service implementations
- Cross-cutting concerns

## 🛠️ Technologies

- **.NET 9.0** - Target framework
- **MediatR 12.4.0** - CQRS and mediator pattern
- **FluentValidation 12.0.0** - Input validation
- **Entity Framework Core 9.0.0** - Data access
- **Microsoft.Extensions.*** - Logging, DI, and hosting abstractions

## 📦 Installation

### From Source
```bash
git clone <repository-url>
cd building_blocks
dotnet restore
dotnet build
```

### As NuGet Package (Application Layer)
```bash
dotnet add package BuildingBlocks.Application
```

## 🔧 Usage

### Setting up Application Services

```csharp
using BuildingBlocks.Application;

// In your Program.cs or Startup.cs
builder.Services.AddApplicationServices();
```

This will register:
- MediatR with all behaviors
- FluentValidation validators
- All application services

### MediatR Pipeline Behaviors Order

The behaviors are registered in the following order:
1. `LoggingBehavior` - Logs request start/completion
2. `UnhandledExceptionBehavior` - Catches unhandled exceptions
3. `AuthorizationBehavior` - Validates user permissions
4. `ValidationBehavior` - Validates request input
5. `PerformanceBehavior` - Monitors execution time

### Example Request Handler

```csharp
public class GetUserQuery : IRequest<Result<User>>
{
    public int UserId { get; set; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, Result<User>>
{
    public async Task<Result<User>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Handler logic here
        // Logging, validation, authorization, and performance monitoring
        // are handled automatically by the pipeline behaviors
    }
}
```

## 📁 Project Structure

```
BuildingBlocks/
├── src/
│   ├── BuildingBlocks.Domain/
│   │   ├── Entities/
│   │   ├── Events/
│   │   ├── ValueObjects/
│   │   └── Common/
│   ├── BuildingBlocks.Application/
│   │   ├── Behaviors/
│   │   ├── Abstractions/
│   │   ├── Extensions/
│   │   ├── Models/
│   │   ├── Security/
│   │   └── DependencyInjection.cs
│   └── BuildingBlocks.Infrastructure/
├── Directory.Build.props
├── Directory.Packages.props
├── BuildingBlocks.sln
└── README.md
```

## ⚙️ Configuration

### Package Management
The solution uses centralized package management via `Directory.Packages.props`. Package versions are defined centrally and referenced without versions in individual projects.

### Build Configuration
Common build settings are defined in `Directory.Build.props`:
- Warnings as errors
- Nullable reference types enabled
- XML documentation generation
- Implicit usings enabled

## 🧪 Development Commands

```bash
# Build the entire solution
dotnet build

# Build specific project
dotnet build src/BuildingBlocks.Application

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

## 🤝 Contributing

1. Follow Clean Architecture principles
2. Maintain dependency flow (Domain ← Application ← Infrastructure)
3. Add XML documentation for public APIs
4. Ensure all builds pass with zero warnings
5. Write unit tests for new features

## 📄 License

[Add your license information here]

## 🔗 Related Projects

- [Your other related projects or dependencies]