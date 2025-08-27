# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Gloam is a .NET 9.0 roguelike game engine with data-driven entity management and flexible rendering backends. The architecture emphasizes JSON-based entity definitions with schema validation, DryIoc dependency injection, and modular input/rendering systems.

## Essential Commands

### Task Runner (Preferred)
The project uses [Task](https://taskfile.dev) as the primary build tool:

```bash
# Show all available tasks
task

# Full development cycle
task dev                    # restore, build, test

# Individual operations
task build                  # Build solution
task test                   # Run all tests
task test-watch            # Run tests in watch mode
task test-filter FILTER="TestName"  # Run specific tests
task lint                  # Format and lint code

# Coverage analysis
task coverage              # Generate coverage data
task coverage-report       # Generate HTML report
task coverage-open         # Open report in browser

# Schema generation
task schema                # Generate JSON schemas from entities

# CI pipeline
task ci                    # clean, build, test, coverage
```

### Alternative .NET Commands
```bash
dotnet build Gloam.slnx
dotnet test Gloam.slnx
dotnet test --filter "TestName"
dotnet format Gloam.slnx
```

### CLI Tool Usage
```bash
# Build CLI first
dotnet build tools/Gloam.Cli/

# Validate JSON files
./tools/Gloam.Cli/bin/Debug/net9.0/gloam validate path/to/file.json
```

## Architecture Overview

### Core Layers
- **Gloam.Core**: Shared utilities, primitives, JSON handling, input abstractions
- **Gloam.Data**: Entity management, JSON schema validation, content loading
- **Gloam.Runtime**: DryIoc-based host, service configuration, runtime management
- **Gloam.Cli**: ConsoleAppFramework-based command-line tools

### Key Patterns

#### JSON-Driven Entities
All game entities are JSON-defined with automatic schema validation:
- Entities inherit from `BaseGloamEntity`
- JSON schemas auto-generated from C# classes
- Runtime validation using `JsonSchemaValidator`
- Source generation via `GloamDataJsonContext`

#### Input System Architecture
Cross-platform input abstraction supporting Console, MonoGame, etc:
- `IInputDevice` interface with edge detection (`IsDown`, `WasPressed`, `WasReleased`)
- `InputKeyData` struct with modifier support
- Complete key mapping in `Keys` class (200+ keys)
- `BaseInputDevice` provides edge detection logic

#### DryIoc Container Integration
- `GloamHost` manages service lifetime and configuration
- Performance-optimized for high-frequency object creation
- Support for multiple content loader types
- Scoped services for game loop operations

#### CLI Command Structure
- ConsoleAppFramework v5.5.0 for command routing
- Spectre.Console for rich terminal output
- Commands inherit from `BaseCommand`
- Executable named `gloam` via `AssemblyName`

### Configuration System
- `RuntimeConfig`: Game loop timing (turn-based vs real-time)
- `GloamHostConfig`: Service container and logging configuration
- Directory structure managed via `DirectoriesConfig`

## Development Patterns

### Testing Strategy
- Target: 100% code coverage
- Entity JSON serialization tests auto-generated
- Schema validation tests for all entity types
- Coverage reports in `coverage/html/index.html`

### Code Quality
- .NET 9.0 with C# preview features
- Nullable reference types enabled
- .NET analyzers with recommended rules
- Source Link enabled for debugging

### Special Claude Commands
- **`test_check`**: When the user writes "test_check", automatically analyze the codebase for missing tests and implement them to maintain 100% coverage target
- **`fix_comments`**: When the user writes "fix_comments", search for classes/structs/records that don't have XML documentation comments and add /// English comments to them

## Project Structure Notes

- Solution uses `.slnx` format (modern solution format)
- `Directory.Build.props` centralizes MSBuild configuration
- Tests organized by namespace hierarchy
- JSON schemas auto-generated to `tests/*/Schema/` directories
- Coverage data collected to `coverage/` directory

## Key Dependencies
- **DryIoc**: Dependency injection container
- **Serilog**: Structured logging
- **ConsoleAppFramework**: CLI command framework
- **Spectre.Console**: Rich terminal UI
- **System.Text.Json**: JSON serialization with source generation