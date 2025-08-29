# Getting Started with Gloam

Welcome to Gloam! This guide will help you get up and running with the Gloam roguelike game engine quickly.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 9.0 SDK** - Download from [Microsoft's official site](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Task Runner** (recommended) - Install from [Task's website](https://taskfile.dev)
- **Git** - For cloning the repository

## Quick Setup

### 1. Clone and Build

```bash
# Clone the repository
git clone https://github.com/tgiachi/gloam.git
cd gloam

# Build the entire solution
task build

# Run tests to ensure everything works
task test
```

### 2. Run the Demo

```bash
# Run the interactive demo
task demo

# Or using dotnet directly
dotnet run --project src/Gloam.Demo
```

### 3. Explore the Codebase

The Gloam engine is organized into several key projects:

- **`Gloam.Core`** - Core interfaces, primitives, and utilities
- **`Gloam.Runtime`** - Game host, dependency injection, and game loop
- **`Gloam.Data`** - Entity management and content loading
- **`Gloam.Console.Render`** - Console-based rendering backend
- **`Gloam.Core.Ui`** - UI controls and components
- **`Gloam.Demo`** - Example implementation

## Your First Gloam Game

Here's a minimal example to get you started:

```csharp
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Console.Render.Rendering;

class Program
{
    static async Task Main()
    {
        // Configure the game host
        var config = new GloamHostConfig
        {
            RootDirectory = Directory.GetCurrentDirectory(),
            EnableConsoleLogging = true
        };

        // Create and initialize the host
        await using var host = new GloamHost(config);

        // Set up console rendering
        var renderer = new ConsoleRenderer();
        host.SetRenderer(renderer);

        // Run the game loop
        await host.RunAsync(
            keepRunning: () => true,
            fixedStep: TimeSpan.FromMilliseconds(16),
            CancellationToken.None
        );
    }
}
```

## Next Steps

1. **Read the Architecture Guide** - Understand how Gloam components work together
2. **Try the Tutorials** - Follow step-by-step guides for common tasks
3. **Explore the Demo** - See Gloam in action with working examples
4. **Check the API Reference** - Detailed documentation for all classes and methods

## Need Help?

- **Issues** - Report bugs on [GitHub Issues](https://github.com/tgiachi/gloam/issues)
- **Discussions** - Ask questions on [GitHub Discussions](https://github.com/tgiachi/gloam/discussions)
- **Documentation** - Browse the full documentation at [docs.gloam.dev](https://yourdocs.github.io/gloam/)

Happy coding with Gloam! 🎮