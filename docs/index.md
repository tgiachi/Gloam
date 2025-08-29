# Gloam Engine Documentation

Welcome to the **Gloam** roguelike game engine documentation.

## Overview

Gloam is a .NET 9.0 roguelike game engine with data-driven entity management and flexible rendering backends. The architecture emphasizes JSON-based entity definitions with schema validation, DryIoc dependency injection, and modular input/rendering systems.

## Key Features

- **Data-Driven Architecture**: JSON-based entity definitions with schema validation
- **Modular Design**: Flexible input and rendering backends (Console, OpenGL, etc.)
- **High Performance**: Optimized game loop with precise timing using `Stopwatch.GetTimestamp()`
- **Dependency Injection**: DryIoc container with performance optimizations
- **Cross-Platform**: Built on .NET 9.0 for Windows, macOS, and Linux

## Getting Started

### Installation

```bash
dotnet tool install -g Gloam.Cli --add-source ./packages
```

### Basic Usage

```csharp
var config = new GloamHostConfig
{
    RootDirectory = "content/",
    LogLevel = LogLevelType.Information
};

var host = new GloamHost(config);
await host.StartAsync();
await host.RunAsync(() => game.IsRunning, TimeSpan.FromMilliseconds(16), ct);
```

## Architecture

### Core Components

- **Gloam.Core**: Core functionality including input, rendering interfaces, and utilities  
- **Gloam.Data**: Entity management, content loading, and JSON serialization
- **Gloam.Runtime**: Game loop, dependency injection, and host services
- **Gloam.Cli**: Command-line tools for validation and development

### Game Loop

The engine features a flexible game loop that supports both:
- **Real-time gameplay**: Fixed timestep simulation with configurable frame rate
- **Turn-based gameplay**: Event-driven updates triggered by player input

## Development

### Building from Source

```bash
git clone https://github.com/tgiachi/gloam.git
cd gloam
task build
```

### Running Tests

```bash
task test
task coverage-report
```

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/tgiachi/gloam/blob/main/LICENSE) file for details.