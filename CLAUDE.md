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

# Documentation generation
task docs-build            # Generate DocFX documentation
task docs-serve            # Serve documentation locally
task docs-open             # Open documentation in browser
task docs-clean            # Clean documentation artifacts

# Schema generation
task schema                # Generate JSON schemas from entities

# CI pipeline
task ci                    # clean, build, test, coverage, docs
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
- **Gloam.Core**: Shared utilities, primitives, JSON handling, input abstractions, layer rendering interfaces
- **Gloam.Data**: Entity management, JSON schema validation, content loading
- **Gloam.Runtime**: DryIoc-based host, service configuration, runtime management, layer rendering services
- **Gloam.Console.Render**: Console-based rendering implementation with ANSI color support
- **Gloam.Demo**: Executable demo application showcasing console rendering
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

#### Layer-Based Rendering System
Modern rendering architecture supporting multiple backends:
- `ILayerRenderer` interface for prioritized layer rendering
- `LayerRenderingManager` handles layer lifecycle and rendering order
- `BaseLayerRenderer` abstract class with template method pattern
- Console backend with `ConsoleRenderer`, `ConsoleSurface`, and `ConsoleInputDevice`
- Transparent background support via alpha channel checking
- Frame timing utilities in `FrameInfoUtils` and `RenderLayerContext`

#### CLI Command Structure
- ConsoleAppFramework v5.5.0 for command routing
- Spectre.Console for rich terminal output
- Commands inherit from `BaseCommand`
- Executable named `gloam` via `AssemblyName`

### Configuration System
- `RuntimeConfig`: Game loop timing (default: 15ms render step ≈67 FPS, 0ms simulation for turn-based)
- `GameLoopConfig`: Modern game loop configuration with `KeepRunning` delegate and timing control
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

### Game Loop Architecture
The engine implements a flexible game loop with precise timing:
- Uses `Stopwatch.GetTimestamp()` for high-precision timing (not `Stopwatch` instances)
- Supports both real-time and turn-based gameplay modes
- Input polling integrated with `IInputDevice.Poll()` and `EndFrame()`
- Rendering managed through `IRenderer.BeginDraw()` / `EndDraw()` pattern
- Layer-based rendering with `LayerRenderingManager.RenderAllLayersAsync()`
- Frame timing calculation fixed to start from game loop initialization (not system boot)
- Configuration via `GameLoopConfig` record in `Gloam.Runtime.Config` namespace
- KISS principle preferred over complex SOLID architectures for maintainability

### Color System
Comprehensive color management for roguelike rendering:
- `Color` struct with RGBA components and hex string support
- `Colors` static class with 60+ predefined colors including roguelike-specific colors
- Support for transparency via alpha channel (alpha=0 treated as transparent in console rendering)
- Console color mapping with RGB to `ConsoleColor` conversion
- Background color bleed prevention in console rendering

### Documentation System
- **DocFX Integration**: Custom documentation generation with alba/gloam dawn theme
- **Theme Colors**: Dawn gradient palette (pink, orange, yellow, purple, blue, mint)
- **Auto-generated API docs**: XML comments automatically extracted to documentation
- **Custom styling**: Located in `docs/styles/main.css` with CSS variables for theme consistency
- **Logo integration**: SVG logo with dawn gradient colors in `docs/images/logo.svg`

### Development Best Practices
- **Always build after modifications**: After making any code changes, **ALWAYS** run `dotnet build Gloam.slnx` as the final step to check for compilation errors. This prevents accumulating build errors and catches issues early.
- **KISS over SOLID**: Prefer simple, direct implementations over complex interface hierarchies
- **XML Documentation**: All public types must have /// English comments explaining their purpose

## Project Structure Notes

- Solution uses `.slnx` format (modern solution format)
- `Directory.Build.props` centralizes MSBuild configuration
- Tests organized by namespace hierarchy
- JSON schemas auto-generated to `tests/*/Schema/` directories
- Coverage data collected to `coverage/` directory

## Key Dependencies
- **DryIoc**: Dependency injection container
- **Serilog**: Structured logging with file and console sinks
- **ConsoleAppFramework**: CLI command framework
- **Spectre.Console**: Rich terminal UI
- **System.Text.Json**: JSON serialization with source generation

## Recent Major Features (v0.6.0)
- **Layer-Based Rendering System**: Complete implementation with `ILayerRenderer`, `LayerRenderingManager`, and `BaseLayerRenderer`
- **Console Rendering Backend**: Full console rendering support with `ConsoleRenderer`, `ConsoleSurface`, and `ConsoleInputDevice`
- **Color System**: Comprehensive color management with `Colors` class and transparency support
- **Frame Timing Fix**: Corrected frame number calculation to start from game loop initialization
- **Demo Application**: Working demo showcasing console rendering with status display
- **Improved Testing**: All 520 tests passing with proper mock setup for `IsVisible` property
- **CI/CD Optimization**: `validate-cli-tool` job now runs only on pull requests to main branch