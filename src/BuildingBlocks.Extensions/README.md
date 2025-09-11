# BuildingBlocks.Extensions

A comprehensive .NET library providing essential extension methods for common development scenarios. This package contains utility extensions for XML processing, date operations, and other common programming tasks.

## Features

### XML Extensions
- **ToXml()** - Serialize objects to XML string with customizable encoding and formatting
- **FromXml<T>()** - Deserialize XML string to strongly-typed objects
- **TryFromXml<T>()** - Safe XML deserialization with error handling
- **IsValidXml()** - Validate XML string format
- **FormatXml()** - Format XML with proper indentation
- **RemoveNamespaces()** - Strip XML namespaces for simplified processing

### DateTime Extensions
- **EachDay()** - Generate sequence of consecutive dates within a range
  - Useful for batch processing and parallel external service calls
  - Memory-efficient lazy evaluation with yield return
  - Perfect for splitting date ranges into individual days

## Installation

```bash
dotnet add package BuildingBlocks.Extensions
```

## Usage Examples

### XML Extensions

```csharp
using BuildingBlocks.Extensions;

// Serialize object to XML
var person = new Person { Name = "John", Age = 30 };
string xml = person.ToXml();

// Deserialize XML to object
var restored = xml.FromXml<Person>();

// Safe deserialization
if (xmlString.TryFromXml<Person>(out var result))
{
    // Process result
}

// Validate XML
if (xmlString.IsValidXml())
{
    // Process valid XML
}
```

### DateTime Extensions

```csharp
using BuildingBlocks.Extensions;

// Generate date range for parallel processing
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 1, 10);

await Task.WhenAll(
    startDate.EachDay(endDate)
        .Select(date => ProcessOrdersForDate(date))
);

// Or iterate through dates
foreach (var date in startDate.EachDay(endDate))
{
    Console.WriteLine($"Processing {date:yyyy-MM-dd}");
}
```

## Requirements

- .NET 9.0 or later
- No external dependencies for core functionality

## License

This project is licensed under the MIT License.