// BuildingBlocks.Tools - CLI tool for BuildingBlocks utilities
// Similar to dotnet ef, provides commands for code generation and management
//
// Usage:
//   bb-tools env generate -a <assembly.dll> -o .env.example
//
// Commands:
//   env generate - Scans assemblies for IOptions implementations and generates .env files

using System.CommandLine;
using BuildingBlocks.Tools.Commands;

var rootCommand = new RootCommand("BuildingBlocks Tools - CLI utilities for BuildingBlocks projects");

// Environment variable management commands
// Includes: generate - creates .env files from IOptions implementations
var envCommand = new Command("env", "Environment variable management commands");
envCommand.AddCommand(EnvGenerateCommand.Create());
rootCommand.AddCommand(envCommand);

return await rootCommand.InvokeAsync(args);
