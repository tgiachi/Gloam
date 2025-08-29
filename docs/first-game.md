# Your First Gloam Game

This tutorial will guide you through creating your first simple game with Gloam.

## Prerequisites

- Gloam installed and configured
- Basic C# knowledge
- A text editor or IDE

## Step 1: Project Setup

Create a new console application:

```bash
dotnet new console -n MyFirstGloamGame
cd MyFirstGloamGame
dotnet add package Gloam.Runtime
dotnet add package Gloam.Console.Render
```

## Step 2: Basic Game Structure

Create a simple game that displays "Hello World" and waits for input:

```csharp
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Console.Render.Rendering;
using Gloam.Console.Render.Input;

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

        // Create host and renderer
        await using var host = new GloamHost(config);
        var renderer = new ConsoleRenderer();
        var inputDevice = new ConsoleInputDevice();

        host.SetRenderer(renderer);
        host.SetInputDevice(inputDevice);

        // Create a simple game scene
        var gameScene = new HelloWorldScene(inputDevice);

        // Register and switch to the scene
        var sceneManager = host.SceneManager;
        sceneManager.RegisterScene(gameScene);
        await sceneManager.SwitchToSceneAsync("HelloWorld");

        // Run the game loop
        var loopConfig = new GameLoopConfig
        {
            KeepRunning = () => !gameScene.ShouldExit,
            RenderStep = TimeSpan.FromMilliseconds(16)
        };

        await host.RunAsync(loopConfig, CancellationToken.None);
    }
}

// Simple game scene
public class HelloWorldScene : BaseScene
{
    private readonly IInputDevice _inputDevice;
    public bool ShouldExit { get; private set; }

    public HelloWorldScene(IInputDevice inputDevice)
    {
        _inputDevice = inputDevice;
    }

    public override async Task UpdateAsync(FrameInfo frameInfo, CancellationToken ct)
    {
        if (_inputDevice.WasPressed(Keys.Escape) || _inputDevice.WasPressed(Keys.Q))
        {
            ShouldExit = true;
        }
    }

    public override void Render(IRenderer renderer)
    {
        // Clear screen
        renderer.Clear();

        // Draw title
        renderer.DrawText("Hello, Gloam!", new Position(10, 5), Colors.Yellow);

        // Draw instructions
        renderer.DrawText("Press ESC or Q to exit", new Position(10, 7), Colors.White);

        // Draw a simple border
        for (int x = 0; x < 40; x++)
        {
            renderer.DrawText("-", new Position(x, 0), Colors.Cyan);
            renderer.DrawText("-", new Position(x, 20), Colors.Cyan);
        }

        for (int y = 0; y < 20; y++)
        {
            renderer.DrawText("|", new Position(0, y), Colors.Cyan);
            renderer.DrawText("|", new Position(40, y), Colors.Cyan);
        }
    }
}
```

## Step 3: Running Your Game

1. Save the code to `Program.cs`
2. Run the application:

```bash
dotnet run
```

You should see:
- A bordered window with "Hello, Gloam!" displayed
- Instructions to press ESC or Q to exit
- The game loop running at 60 FPS

## Step 4: Adding More Features

Let's enhance the game with a simple player character:

```csharp
// Add to HelloWorldScene
private Position _playerPos = new Position(20, 10);

public override async Task UpdateAsync(FrameInfo frameInfo, CancellationToken ct)
{
    // Handle input
    if (_inputDevice.WasPressed(Keys.Escape) || _inputDevice.WasPressed(Keys.Q))
    {
        ShouldExit = true;
    }

    // Move player
    if (_inputDevice.IsDown(Keys.W) || _inputDevice.IsDown(Keys.UpArrow))
    {
        _playerPos = new Position(_playerPos.X, Math.Max(1, _playerPos.Y - 1));
    }
    if (_inputDevice.IsDown(Keys.S) || _inputDevice.IsDown(Keys.DownArrow))
    {
        _playerPos = new Position(_playerPos.X, Math.Min(19, _playerPos.Y + 1));
    }
    if (_inputDevice.IsDown(Keys.A) || _inputDevice.IsDown(Keys.LeftArrow))
    {
        _playerPos = new Position(Math.Max(1, _playerPos.X - 1), _playerPos.Y);
    }
    if (_inputDevice.IsDown(Keys.D) || _inputDevice.IsDown(Keys.RightArrow))
    {
        _playerPos = new Position(Math.Min(39, _playerPos.X + 1), _playerPos.Y);
    }
}

public override void Render(IRenderer renderer)
{
    // Clear screen
    renderer.Clear();

    // Draw title
    renderer.DrawText("Hello, Gloam!", new Position(10, 5), Colors.Yellow);

    // Draw instructions
    renderer.DrawText("Use WASD or Arrow Keys to move, ESC/Q to exit", new Position(10, 7), Colors.White);

    // Draw player
    renderer.DrawText("@", _playerPos, Colors.Cyan);

    // Draw border (same as before)
    // ... border drawing code ...
}
```

## Step 5: Adding Entity System

Now let's add a simple entity using Gloam's data-driven system:

```csharp
// Create a content/entities/player.json file:
{
  "id": "player",
  "name": "Player",
  "visual": {
    "glyph": "@",
    "foreground": "#00FFFF"
  }
}

// Update the scene to load the entity
private Entity? _playerEntity;

public override async Task InitializeAsync()
{
    // Load player entity
    var entityLoader = new EntityDataLoader();
    _playerEntity = await entityLoader.LoadEntityAsync("player");
}

public override void Render(IRenderer renderer)
{
    // ... existing code ...

    // Draw player using entity data
    if (_playerEntity != null)
    {
        renderer.DrawText(
            _playerEntity.Visual.Glyph,
            _playerPos,
            _playerEntity.Visual.GetForegroundColor()
        );
    }
}
```

## What's Next?

You've created your first Gloam game! Here are some next steps:

1. **Explore the Demo**: Run `dotnet run --project src/Gloam.Demo` to see more features
2. **Read Tutorials**: Check out the tutorial sections for specific topics
3. **Study Examples**: Look at the code samples for advanced patterns
4. **Join Community**: Connect with other Gloam developers

## Troubleshooting

### Common Issues

**Game doesn't start:**
- Ensure all NuGet packages are restored: `dotnet restore`
- Check that you're using .NET 9.0: `dotnet --version`

**Input not working:**
- Make sure you're running in a proper terminal/console
- Some terminals don't support all ANSI escape sequences

**Performance issues:**
- The game runs at 60 FPS by default
- You can adjust `RenderStep` in `GameLoopConfig` for different frame rates

### Getting Help

- Check the API Reference for detailed documentation
- Look at Examples for working code
- Report issues on [GitHub](https://github.com/tgiachi/gloam/issues)

Congratulations on creating your first Gloam game! 🎮