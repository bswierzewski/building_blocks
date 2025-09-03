# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

BuildingBlocks is a .NET 9.0 solution implementing Clean Architecture with three main layers:

- **Domain** (`src/BuildingBlocks.Domain`) - Core business entities and domain logic, with no external dependencies
- **Application** (`src/BuildingBlocks.Application`) - Application services, use cases, and interfaces, depends on Domain
- **Infrastructure** (`src/BuildingBlocks.Infrastructure`) - External concerns like data access and third-party integrations, depends on both Domain and Application

## Project Configuration

- **Target Framework**: .NET 9.0
- **Package Management**: Centralized via `Directory.Packages.props` with `ManagePackageVersionsCentrally` enabled
- **Build Settings**: Configured in `Directory.Build.props` with warnings as errors, nullable enabled, and documentation generation
- **Dependencies**: Clean Architecture dependency flow is enforced (Domain ← Application ← Infrastructure)

## Common Commands

```bash
# Build the entire solution
dotnet build

# Build specific project
dotnet build src/BuildingBlocks.Domain

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

## Development Guidelines

- Follow Clean Architecture principles: Domain should have no dependencies, Application depends only on Domain, Infrastructure depends on both
- All projects use nullable reference types and treat warnings as errors
- Package versions are managed centrally in Directory.Packages.props
- Use implicit usings (enabled globally)