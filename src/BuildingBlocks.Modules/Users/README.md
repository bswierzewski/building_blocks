# BuildingBlocks.Users Module

Comprehensive user management module for modular monolith applications with support for multiple identity providers, role-based access control (RBAC), and just-in-time (JIT) user provisioning.

## Features

- **Multi-Provider Authentication**: Support for Clerk, Auth0, and other JWT-based identity providers
- **Just-In-Time User Provisioning**: Automatically creates users on first login
- **Role-Based Access Control (RBAC)**: Hierarchical roles with granular permissions
- **Permission Management**: Module-based permission discovery and seeding
- **Clean Architecture**: Domain-driven design with CQRS pattern
- **Standard Claims**: Uses standard `ClaimTypes` for provider-agnostic implementation

## Architecture

The module is organized in three layers following Clean Architecture principles:

### Domain Layer
- **User Aggregate**: Core user entity with external identities, roles, and permissions
- **Role Entity**: Defines user roles with associated permissions
- **Permission Entity**: Granular permission definitions
- **Value Objects**: `UserId`, `Email`, `ExternalIdentity`

### Application Layer
- **Commands**: `AssignRoleToUser`, `RemoveRoleFromUser`
- **Queries**: `GetCurrentUser`, `GetAllPermissions`, `GetAllRoles`
- **Interfaces**: `IUserProvisioningService`, `IUser`, `IModule`

### Infrastructure Layer
- **Services**: User provisioning, user context access
- **JWT Extensions**: Provider-specific authentication (`AddClerkJwtBearer`, `AddAuth0JwtBearer`)
- **Database**: Entity Framework Core with PostgreSQL support
- **Hosted Services**: Automatic role and permission seeding at startup

## Installation

### 1. Install NuGet Package

```bash
dotnet add package BuildingBlocks.Users
```

### 2. Configure Database

Add connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "UsersDb": "Host=localhost;Database=myapp_users;Username=postgres;Password=***"
  }
}
```

### 3. Register Module Services

In your `Program.cs`:

```csharp
using BuildingBlocks.Modules.Users.Infrastructure.Module;

var builder = WebApplication.CreateBuilder(args);

// Add Users module with PostgreSQL
builder.Services.AddUsersModule(builder.Configuration, usePostgreSQL: true);

var app = builder.Build();
```

### 4. Configure Authentication

#### For Clerk:

First, add Clerk configuration to `appsettings.json`:

```json
{
  "Clerk": {
    "Authority": "https://your-clerk-domain.clerk.accounts.dev",
    "Audience": "your-audience" // Optional
  }
}
```

Then, configure Clerk options and authentication in `Program.cs`:

```csharp
using BuildingBlocks.Modules.Users.Infrastructure.Module;
using BuildingBlocks.Modules.Users.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Clerk options from configuration
builder.Services.AddClerkOptions(builder.Configuration);

// Add authentication with Clerk
builder.Services
    .AddAuthentication()
    .AddClerkJwtBearer();

var app = builder.Build();
```

The `AddClerkOptions` method:
- Binds configuration from the `Clerk` section
- Validates required fields (`Authority` is required, `Audience` is optional)
- Validates on application start to fail fast if configuration is missing

#### For Auth0:

First, add Auth0 configuration to `appsettings.json`:

```json
{
  "Auth0": {
    "Authority": "https://your-tenant.auth0.com/",
    "Audience": "your-api-identifier"
  }
}
```

Then, configure Auth0 options and authentication in `Program.cs`:

```csharp
using BuildingBlocks.Modules.Users.Infrastructure.Module;
using BuildingBlocks.Modules.Users.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Auth0 options from configuration
builder.Services.AddAuth0Options(builder.Configuration);

// Add authentication with Auth0
builder.Services
    .AddAuthentication()
    .AddAuth0JwtBearer();

var app = builder.Build();
```

The `AddAuth0Options` method:
- Binds configuration from the `Auth0` section
- Validates required fields (`Authority` and `Audience` are both required)
- Validates on application start to fail fast if configuration is missing

### 5. Run Database Migrations

```bash
dotnet ef migrations add InitialCreate --project YourInfrastructureProject
dotnet ef database update --project YourInfrastructureProject
```

## Usage

### Accessing Current User Information

Inject `IUser` interface to access current authenticated user:

```csharp
using BuildingBlocks.Application.Abstractions;

public class MyService
{
    private readonly IUser _user;

    public MyService(IUser user)
    {
        _user = user;
    }

    public async Task DoSomething()
    {
        var userId = _user.Id; // Internal Guid user ID
        var email = _user.Email;
        var fullName = _user.FullName;
        var isAuthenticated = _user.IsAuthenticated;

        // Check permissions
        if (_user.HasPermission("users.delete"))
        {
            // Delete user logic
        }

        // Check roles
        if (_user.IsInRole("Admin"))
        {
            // Admin logic
        }
    }
}
```

### Using CQRS Queries

```csharp
using MediatR;
using BuildingBlocks.Modules.Users.Application.Queries.GetCurrentUser;

public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<CurrentUserDto> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery();
        return await _mediator.Send(query);
    }
}
```

### Assigning Roles to Users

```csharp
using BuildingBlocks.Modules.Users.Application.Commands.AssignRoleToUser;

public class UserManagementService
{
    private readonly IMediator _mediator;

    public UserManagementService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task MakeUserAdmin(Guid userId, Guid adminRoleId)
    {
        var command = new AssignRoleToUserCommand(userId, adminRoleId);
        await _mediator.Send(command);
    }
}
```

## Defining Module Permissions

Implement `IModule` interface in your custom modules:

```csharp
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Entities;

public class OrdersModule : IModule
{
    public string ModuleName => "Orders";
    public string DisplayName => "Order Management";
    public string? Description => "Manage customer orders";

    public IEnumerable<Permission> GetPermissions()
    {
        return new[]
        {
            Permission.Create("orders.view", "View Orders", ModuleName, "View all orders"),
            Permission.Create("orders.create", "Create Orders", ModuleName, "Create new orders"),
            Permission.Create("orders.edit", "Edit Orders", ModuleName, "Edit existing orders"),
            Permission.Create("orders.delete", "Delete Orders", ModuleName, "Delete orders"),
        };
    }

    public IEnumerable<Role> GetRoles()
    {
        var viewPermission = Permission.Create("orders.view", "View Orders", ModuleName);
        var createPermission = Permission.Create("orders.create", "Create Orders", ModuleName);
        var editPermission = Permission.Create("orders.edit", "Edit Orders", ModuleName);
        var deletePermission = Permission.Create("orders.delete", "Delete Orders", ModuleName);

        var orderManager = Role.Create("OrderManager", "Order Manager", ModuleName);
        orderManager.AddPermission(viewPermission);
        orderManager.AddPermission(createPermission);
        orderManager.AddPermission(editPermission);

        var orderViewer = Role.Create("OrderViewer", "Order Viewer", ModuleName);
        orderViewer.AddPermission(viewPermission);

        return new[] { orderManager, orderViewer };
    }
}
```

Register your module:

```csharp
builder.Services.AddSingleton<IModule, OrdersModule>();
```

The `RolesAndPermissionsHostedService` will automatically discover and seed permissions/roles at startup.

## How It Works

### 1. JWT Token Validation & User Provisioning

When a JWT token arrives:

1. **Token Validation**: Provider-specific extension (`AddClerkJwtBearer` or `AddAuth0JwtBearer`) validates the token
2. **OnTokenValidated Event**:
   - Extracts `sub` claim (external user ID)
   - Calls `IUserProvisioningService.GetUserAsync()` to check if user exists
   - If not found, calls `IUserProvisioningService.AddUserAsync()` for JIT provisioning
3. **Claim Mapping**:
   - Maps provider-specific claims to standard `ClaimTypes` (Email, Name, Role)
   - Adds internal `user_id` claim
   - Enriches with roles and permissions from database
4. **Result**: `ClaimsPrincipal` contains all necessary claims for authorization

### 2. Module Discovery & Permission Seeding

At application startup:

1. **RolesAndPermissionsHostedService** discovers all `IModule` implementations
2. For each module:
   - Extracts permissions via `GetPermissions()`
   - Extracts roles via `GetRoles()`
   - Upserts to database (creates new or updates existing)
3. **Result**: Database contains all permissions and roles from all modules

### 3. Authorization

The enriched `ClaimsPrincipal` can be used with:

- **IUser service**: `_user.HasPermission("users.delete")`
- **Policy-based authorization**: Define policies based on permissions
- **Role-based authorization**: `[Authorize(Roles = "Admin")]`

## Database Schema

### Core Tables

- **Users**: User accounts with email, display name, and timestamps
- **ExternalIdentities**: Links users to external identity providers
- **Roles**: Role definitions with name and module association
- **Permissions**: Permission definitions with name and module association
- **UserRoles**: Many-to-many relationship between Users and Roles
- **RolePermissions**: Many-to-many relationship between Roles and Permissions

## Security Considerations

- **JWT Only Contains External ID**: Roles and permissions are loaded from database, not JWT
- **Token Staleness**: User changes (role assignments) require new login to take effect
- **Race Conditions**: JIT provisioning uses separate Get/Add methods to handle concurrent first logins
- **Standard Claims**: Provider-agnostic implementation using `ClaimTypes`

## Example: Next.js Integration

```typescript
// app/api/auth/me/route.ts
import { NextResponse } from 'next/server';

export async function GET(request: Request) {
  const token = request.headers.get('Authorization')?.replace('Bearer ', '');

  const response = await fetch('https://your-api.com/api/users/me', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  const user = await response.json();
  // Returns: { id, email, displayName, pictureUrl, roles, permissions }

  return NextResponse.json(user);
}
```

## Dependencies

- **BuildingBlocks.Domain**: Core domain abstractions
- **BuildingBlocks.Application**: Application layer abstractions
- **BuildingBlocks.Infrastructure**: Infrastructure implementations
- **Entity Framework Core**: Data access
- **MediatR**: CQRS pattern implementation
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication

## License

This module is part of the BuildingBlocks framework.
