# BuildingBlocks.Domain

A .NET domain layer library providing essential building blocks for Domain-Driven Design (DDD) and Clean Architecture applications with zero external dependencies.

## 📦 Installation

```bash
dotnet add package BuildingBlocks.Domain
```

## 🏗️ What's Included

This package contains the actual implementation of domain building blocks currently available:

### Base Entity Classes

#### `Entity<TId>`
Generic base entity with strongly-typed ID:
```csharp
using BuildingBlocks.Domain.Primitives;

public class User : Entity<int>
{
    // Your properties and logic here
}

// Generic ID types supported
public class Product : Entity<Guid>
{
    // Properties and logic
}
```

#### `AuditableEntity<TId>`
Entity with automatic audit trail support:
```csharp
using BuildingBlocks.Domain.Primitives;

public class Document : AuditableEntity<int>
{
    public string Title { get; set; } = string.Empty;
    
    // Inherits:
    // - Id (TId)
    // - CreatedAt (DateTime) 
    // - CreatedBy (string?)
    // - ModifiedAt (DateTime?)
    // - ModifiedBy (string?)
}
```

#### `AggregateRoot<TId>`
Full-featured aggregate root with domain events and business rule validation:
```csharp
using BuildingBlocks.Domain.Primitives;

public class Order : AggregateRoot<int>
{
    public void ChangeStatus(OrderStatus newStatus)
    {
        // Business rule validation
        CheckRule(new OrderCanChangeStatusRule(Status, newStatus));
        
        Status = newStatus;
        
        // Raise domain event
        AddDomainEvent(new OrderStatusChangedEvent(Id, Status, newStatus));
    }
}
```

### Domain Events

#### `IDomainEvent` Interface
```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
```

#### `DomainEvent` Base Class
```csharp
using BuildingBlocks.Domain.Primitives;

public class UserRegisteredEvent : DomainEvent
{
    public int UserId { get; }
    public string Email { get; }
    
    public UserRegisteredEvent(int userId, string email)
    {
        UserId = userId;
        Email = email;
    }
    
    // Inherits:
    // - Id (Guid) - automatically generated
    // - OccurredOn (DateTime) - set to UtcNow
}
```

### Business Rules

#### `IBusinessRule` Interface
Define domain business rules:
```csharp
using BuildingBlocks.Domain.Rules;

public class UserMustBeAdultRule : IBusinessRule
{
    private readonly DateTime _birthDate;
    
    public UserMustBeAdultRule(DateTime birthDate)
    {
        _birthDate = birthDate;
    }
    
    public string Message => "User must be at least 18 years old.";
    
    public bool IsBroken()
    {
        return DateTime.Today.Year - _birthDate.Year < 18;
    }
}

// Usage in aggregate root:
public class User : AggregateRoot<int>
{
    public void SetBirthDate(DateTime birthDate)
    {
        CheckRule(new UserMustBeAdultRule(birthDate));
        
        BirthDate = birthDate;
    }
}
```

### Type-Safe Enumerations

#### `Enumeration<TEnum>`
Strongly-typed enumeration pattern:
```csharp
using BuildingBlocks.Domain.Primitives;

public class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Draft = new(1, nameof(Draft));
    public static readonly OrderStatus Submitted = new(2, nameof(Submitted));
    public static readonly OrderStatus Approved = new(3, nameof(Approved));
    public static readonly OrderStatus Rejected = new(4, nameof(Rejected));
    
    private OrderStatus(int value, string name) : base(value, name) { }
    
    // Additional domain logic
    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return (this, newStatus) switch
        {
            (var current, var next) when current == Draft => 
                next == Submitted,
            (var current, var next) when current == Submitted => 
                next == Approved || next == Rejected,
            _ => false
        };
    }
}

// Usage:
var allStatuses = OrderStatus.GetAll();
var submitted = OrderStatus.FromValue(2);
var approved = OrderStatus.FromName("Approved");

if (OrderStatus.TryFromValue(5, out var status))
{
    Console.WriteLine($"Found: {status.Name}");
}
```

### Exception Types

#### `DomainException`
Base exception for domain-specific errors:
```csharp
using BuildingBlocks.Domain.Exceptions;

public class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Insufficient funds: requested {requested}, available {available}")
    {
    }
}
```

#### `BusinessRuleValidationException`
Automatically thrown when business rules are violated:
```csharp
// Thrown automatically by CheckRule() method in AggregateRoot
try
{
    user.SetBirthDate(DateTime.Today.AddYears(-10));
}
catch (BusinessRuleValidationException ex)
{
    Console.WriteLine(ex.BrokenRule.Message); // "User must be at least 18 years old."
    Console.WriteLine(ex.BrokenRule.GetType().Name); // "UserMustBeAdultRule"
}
```

### Key Interfaces

#### `IAggregateRoot`
Marker interface for aggregate roots:
```csharp
public interface IAggregateRoot { }
```

#### `IHasDomainEvents`
Interface for entities that can raise domain events:
```csharp
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    void RemoveDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}
```

#### `IAuditable`
Interface for auditable entities:
```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; }
    string? CreatedBy { get; }
    DateTime? ModifiedAt { get; }
    string? ModifiedBy { get; }
}
```

## 📁 Package Structure

```
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IAggregateRoot.cs           # Marker interface for aggregate roots
│   ├── IAuditable.cs               # Auditing contract
│   ├── IDomainEvent.cs             # Domain event contract
│   └── IHasDomainEvents.cs         # Domain event management contract
├── Exceptions/
│   ├── BusinessRuleValidationException.cs  # Business rule violation exception
│   └── DomainException.cs          # Base domain exception
├── Primitives/
│   ├── AggregateRoot.cs            # Full-featured aggregate root
│   ├── AuditableEntity.cs          # Entity with audit support
│   ├── DomainEvent.cs              # Base domain event implementation
│   ├── Entity.cs                   # Basic entity with ID
│   └── Enumeration.cs              # Type-safe enumeration base
└── Rules/
    └── IBusinessRule.cs            # Business rule interface
```

## 🚀 Complete Example

```csharp
using BuildingBlocks.Domain.Primitives;
using BuildingBlocks.Domain.Rules;

// Enumeration
public class AccountType : Enumeration<AccountType>
{
    public static readonly AccountType Checking = new(1, nameof(Checking));
    public static readonly AccountType Savings = new(2, nameof(Savings));
    
    private AccountType(int value, string name) : base(value, name) { }
}

// Business Rules
public class MinimumBalanceRule : IBusinessRule
{
    private readonly decimal _balance;
    private readonly AccountType _accountType;
    
    public MinimumBalanceRule(decimal balance, AccountType accountType)
    {
        _balance = balance;
        _accountType = accountType;
    }
    
    public string Message => $"Minimum balance of {GetMinimumBalance()} required for {_accountType.Name} account.";
    
    public bool IsBroken() => _balance < GetMinimumBalance();
    
    private decimal GetMinimumBalance() => _accountType == AccountType.Savings ? 100m : 0m;
}

// Domain Events
public class AccountCreatedEvent : DomainEvent
{
    public int AccountId { get; }
    public string AccountNumber { get; }
    public AccountType AccountType { get; }
    
    public AccountCreatedEvent(int accountId, string accountNumber, AccountType accountType)
    {
        AccountId = accountId;
        AccountNumber = accountNumber;
        AccountType = accountType;
    }
}

public class BalanceChangedEvent : DomainEvent
{
    public int AccountId { get; }
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }
    
    public BalanceChangedEvent(int accountId, decimal oldBalance, decimal newBalance)
    {
        AccountId = accountId;
        OldBalance = oldBalance;
        NewBalance = newBalance;
    }
}

// Aggregate Root
public class BankAccount : AggregateRoot<int>
{
    public string AccountNumber { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public AccountType AccountType { get; private set; } = AccountType.Checking;
    
    private BankAccount() { } // For EF Core
    
    public BankAccount(string accountNumber, AccountType accountType)
    {
        AccountNumber = accountNumber;
        AccountType = accountType;
        Balance = 0;
        
        AddDomainEvent(new AccountCreatedEvent(Id, accountNumber, accountType));
    }
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Deposit amount must be positive.");
            
        var oldBalance = Balance;
        Balance += amount;
        
        AddDomainEvent(new BalanceChangedEvent(Id, oldBalance, Balance));
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Withdrawal amount must be positive.");
            
        var newBalance = Balance - amount;
        CheckRule(new MinimumBalanceRule(newBalance, AccountType));
        
        var oldBalance = Balance;
        Balance = newBalance;
        
        AddDomainEvent(new BalanceChangedEvent(Id, oldBalance, Balance));
    }
}
```

## ⚡ Key Features

- **Zero Dependencies** - Pure domain layer with no external dependencies
- **Strongly-Typed IDs** - Generic entity base classes support any ID type
- **Automatic Auditing** - Built-in audit trail support with `AuditableEntity`
- **Domain Events** - Built-in domain event support with automatic management
- **Business Rule Validation** - Enforce business rules with clear error messages
- **Type-Safe Enumerations** - Rich enumeration pattern with additional behavior
- **Exception Safety** - Dedicated exception types for domain-specific errors

## 🏗️ Architecture Integration

This library serves as the foundation layer in Clean Architecture with zero dependencies:

```
┌─────────────────────────────────────┐
│        Infrastructure              │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│          Application                │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│    BuildingBlocks.Domain            │ ◄── This Package (Zero Dependencies)
│   (Entities, Events, Rules)         │
└─────────────────────────────────────┘
```

## 🛡️ Best Practices

1. **Inherit from appropriate base classes** based on your needs:
   - `Entity<TId>` for simple entities
   - `AuditableEntity<TId>` when you need audit trails
   - `AggregateRoot<TId>` for domain event support and business rules

2. **Use domain events** to decouple side effects from main business logic

3. **Implement business rules** using `IBusinessRule` for clear validation logic

4. **Use type-safe enumerations** instead of regular enums for rich domain behavior

5. **Keep the domain pure** - this package has zero dependencies by design

## 📄 License

MIT