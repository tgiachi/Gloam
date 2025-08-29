# Code Samples

This document provides practical code examples for common Gloam development tasks.

## Basic Application Setup

### Minimal Gloam Application

```csharp
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Console.Render.Rendering;

class Program
{
    static async Task Main()
    {
        // Create host configuration
        var config = new GloamHostConfig
        {
            RootDirectory = Directory.GetCurrentDirectory(),
            EnableConsoleLogging = true,
            AppName = "My Gloam Game",
            AppVersion = "1.0.0"
        };

        // Create and configure host
        await using var host = new GloamHost(config);

        // Set up console rendering
        var renderer = new ConsoleRenderer();
        host.SetRenderer(renderer);

        // Create input device
        var inputDevice = new ConsoleInputDevice();
        host.SetInputDevice(inputDevice);

        // Run game loop
        var loopConfig = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };

        await host.RunAsync(loopConfig, CancellationToken.None);
    }
}
```

## Scene Management

### Creating Custom Scenes

```csharp
using Gloam.Core.Interfaces;
using Gloam.Runtime.Services;

public class MainMenuScene : BaseScene
{
    private readonly IInputDevice _inputDevice;
    private readonly ISceneManager _sceneManager;
    private int _selectedOption;

    public MainMenuScene(IInputDevice inputDevice, ISceneManager sceneManager)
    {
        _inputDevice = inputDevice;
        _sceneManager = sceneManager;
    }

    public override async Task UpdateAsync(FrameInfo frameInfo, CancellationToken ct)
    {
        // Handle input
        if (_inputDevice.WasPressed(Keys.UpArrow))
        {
            _selectedOption = Math.Max(0, _selectedOption - 1);
        }
        else if (_inputDevice.WasPressed(Keys.DownArrow))
        {
            _selectedOption = Math.Min(2, _selectedOption + 1);
        }
        else if (_inputDevice.WasPressed(Keys.Enter))
        {
            await HandleMenuSelectionAsync(ct);
        }
    }

    private async Task HandleMenuSelectionAsync(CancellationToken ct)
    {
        switch (_selectedOption)
        {
            case 0: // New Game
                var transition = new FadeTransition(
                    TimeSpan.FromSeconds(1),
                    FadeDirection.FadeOut
                );
                await _sceneManager.SwitchToSceneAsync("Game", transition);
                break;
            case 1: // Options
                await _sceneManager.SwitchToSceneAsync("Options");
                break;
            case 2: // Exit
                Environment.Exit(0);
                break;
        }
    }

    public override void Render(IRenderer renderer)
    {
        // Render menu options
        renderer.DrawText("My Gloam Game", new Position(10, 5), Colors.White);

        string[] options = { "New Game", "Options", "Exit" };
        for (int i = 0; i < options.Length; i++)
        {
            var color = i == _selectedOption ? Colors.Yellow : Colors.White;
            var position = new Position(10, 8 + i);
            renderer.DrawText(options[i], position, color);
        }
    }
}
```

### Scene Registration

```csharp
// In your game initialization
public async Task InitializeScenesAsync()
{
    var sceneManager = Container.Resolve<ISceneManager>();

    // Register scenes
    sceneManager.RegisterScene(new MainMenuScene(
        Container.Resolve<IInputDevice>(),
        sceneManager
    ));

    sceneManager.RegisterScene(new GameScene(
        Container.Resolve<IInputDevice>(),
        Container.Resolve<IEntityDataLoader>()
    ));

    // Set initial scene
    await sceneManager.SwitchToSceneAsync("MainMenu");
}
```

## Entity System Usage

### Loading Entities

```csharp
public class EntityManager
{
    private readonly IEntityDataLoader _entityLoader;
    private readonly Dictionary<string, Entity> _loadedEntities = new();

    public EntityManager(IEntityDataLoader entityLoader)
    {
        _entityLoader = entityLoader;
    }

    public async Task<Entity> LoadEntityAsync(string entityId)
    {
        if (_loadedEntities.TryGetValue(entityId, out var cachedEntity))
        {
            return cachedEntity;
        }

        try
        {
            var entity = await _entityLoader.LoadEntityAsync(entityId);
            _loadedEntities[entityId] = entity;
            return entity;
        }
        catch (EntityValidationException ex)
        {
            Log.Error("Failed to load entity {EntityId}: {Errors}",
                     entityId, string.Join(", ", ex.ValidationErrors));
            throw;
        }
    }

    public async Task<IEnumerable<Entity>> LoadEntitiesAsync(IEnumerable<string> entityIds)
    {
        var tasks = entityIds.Select(id => LoadEntityAsync(id));
        return await Task.WhenAll(tasks);
    }
}
```

### Creating Custom Entities

```csharp
// Define entity class
public class PlayerEntity : BaseGloamEntity
{
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Level { get; set; }
    public List<string> Inventory { get; set; } = new();

    public void TakeDamage(int damage)
    {
        Health = Math.Max(0, Health - damage);
        if (Health == 0)
        {
            OnPlayerDied();
        }
    }

    public void Heal(int amount)
    {
        Health = Math.Min(MaxHealth, Health + amount);
    }

    private void OnPlayerDied()
    {
        // Handle player death
        Log.Information("Player {Name} has died", Name);
    }
}

// JSON entity definition
{
  "id": "player_knight",
  "name": "Knight",
  "description": "A brave knight in shining armor",
  "visual": {
    "glyph": "@",
    "foreground": "#C0C0C0",
    "background": null
  },
  "properties": {
    "health": 150,
    "maxHealth": 150,
    "level": 1,
    "inventory": ["sword", "shield", "health_potion"]
  }
}
```

## UI Controls

### Creating a Character Stats Panel

```csharp
public class CharacterStatsPanel : BaseControl
{
    private readonly PlayerEntity _player;

    public CharacterStatsPanel(Position position, Size size, PlayerEntity player)
        : base(position, size)
    {
        _player = player;
    }

    protected override void RenderOverride(IRenderer renderer)
    {
        // Draw panel background
        renderer.FillRectangle(Bounds, Colors.DarkGray);

        // Draw border
        renderer.DrawRectangle(Bounds, Colors.White);

        // Draw title
        renderer.DrawText("Character Stats", Position + new Position(2, 1), Colors.Yellow);

        // Draw stats
        int y = 3;
        DrawStat(renderer, "Name:", _player.Name, y++);
        DrawStat(renderer, "Health:", $"{_player.Health}/{_player.MaxHealth}", y++);
        DrawStat(renderer, "Level:", _player.Level.ToString(), y++);

        // Draw inventory
        renderer.DrawText("Inventory:", Position + new Position(2, y++), Colors.Cyan);
        for (int i = 0; i < Math.Min(_player.Inventory.Count, 5); i++)
        {
            renderer.DrawText($"- {_player.Inventory[i]}",
                           Position + new Position(4, y++), Colors.White);
        }
    }

    private void DrawStat(IRenderer renderer, string label, string value, int y)
    {
        renderer.DrawText(label, Position + new Position(2, y), Colors.White);
        renderer.DrawText(value, Position + new Position(15, y), Colors.Yellow);
    }
}
```

### Using UI Controls

```csharp
public class GameHud : BaseControl
{
    private readonly CharacterStatsPanel _statsPanel;
    private readonly ProgressBar _healthBar;

    public GameHud(PlayerEntity player) : base(Position.Zero, new Size(80, 25))
    {
        // Create stats panel
        _statsPanel = new CharacterStatsPanel(
            new Position(60, 1),
            new Size(18, 15),
            player
        );

        // Create health bar
        _healthBar = new ProgressBar(
            new Position(1, 22),
            new Size(30, 1)
        )
        {
            Value = player.Health,
            MaxValue = player.MaxHealth,
            ForegroundColor = Colors.Red,
            BackgroundColor = Colors.DarkRed
        };

        // Add child controls
        AddChild(_statsPanel);
        AddChild(_healthBar);
    }

    protected override void RenderOverride(IRenderer renderer)
    {
        // Child controls render automatically
        // Additional HUD rendering can go here
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        _healthBar.Value = currentHealth;
        _healthBar.MaxValue = maxHealth;
    }
}
```

## Input Handling

### Advanced Input Processing

```csharp
public class InputHandler
{
    private readonly IInputDevice _inputDevice;
    private readonly Dictionary<InputKeyData, Action> _keyBindings = new();

    public InputHandler(IInputDevice inputDevice)
    {
        _inputDevice = inputDevice;
        SetupKeyBindings();
    }

    private void SetupKeyBindings()
    {
        // Movement
        _keyBindings[Keys.W] = () => MovePlayer(Direction.Up);
        _keyBindings[Keys.S] = () => MovePlayer(Direction.Down);
        _keyBindings[Keys.A] = () => MovePlayer(Direction.Left);
        _keyBindings[Keys.D] = () => MovePlayer(Direction.Right);

        // Actions
        _keyBindings[Keys.Space] = () => PerformAction();
        _keyBindings[Keys.I] = () => OpenInventory();
        _keyBindings[Keys.Escape] = () => PauseGame();
    }

    public void ProcessInput()
    {
        foreach (var binding in _keyBindings)
        {
            if (_inputDevice.WasPressed(binding.Key))
            {
                binding.Value();
            }
        }

        // Handle mouse input
        if (_inputDevice.WasPressed(MouseButtons.Left))
        {
            HandleMouseClick(_inputDevice.MouseState.Position);
        }
    }

    private void MovePlayer(Direction direction)
    {
        // Implement player movement
        Log.Debug("Moving player {Direction}", direction);
    }

    private void PerformAction()
    {
        // Implement action logic
        Log.Debug("Performing action");
    }

    private void OpenInventory()
    {
        // Open inventory UI
        Log.Debug("Opening inventory");
    }

    private void PauseGame()
    {
        // Pause game logic
        Log.Debug("Pausing game");
    }

    private void HandleMouseClick(Position position)
    {
        // Handle mouse click at position
        Log.Debug("Mouse clicked at {Position}", position);
    }
}
```

## Configuration Management

### Advanced Configuration Setup

```csharp
public class GameConfiguration
{
    public static GloamHostConfig CreateHostConfig()
    {
        var config = new GloamHostConfig
        {
            RootDirectory = GetContentDirectory(),
            EnableConsoleLogging = IsDevelopment(),
            EnableFileLogging = true,
            LogLevel = GetLogLevel(),
            LoaderType = ContentLoaderType.FileSystem
        };

        // Apply environment-specific settings
        if (IsProduction())
        {
            config.EnableConsoleLogging = false;
            config.LogLevel = LogLevelType.Warning;
        }

        return config;
    }

    public static GameLoopConfig CreateGameLoopConfig()
    {
        return new GameLoopConfig
        {
            KeepRunning = () => !GameState.IsGameOver,
            RenderStep = GetTargetFrameTime(),
            SimulationStep = TimeSpan.Zero,
            SleepTime = TimeSpan.FromMilliseconds(1),
            LoopMode = LoopMode.Internal
        };
    }

    private static string GetContentDirectory()
    {
        var exePath = AppContext.BaseDirectory;
        return Path.Combine(exePath, "Content");
    }

    private static bool IsDevelopment()
    {
        #if DEBUG
        return true;
        #else
        return false;
        #endif
    }

    private static bool IsProduction()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                        ?? "Production";

        return environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }

    private static LogLevelType GetLogLevel()
    {
        var logLevelStr = Environment.GetEnvironmentVariable("GLOAM_LOG_LEVEL") ?? "Information";
        return Enum.Parse<LogLevelType>(logLevelStr, true);
    }

    private static TimeSpan GetTargetFrameTime()
    {
        var targetFps = int.Parse(Environment.GetEnvironmentVariable("GLOAM_TARGET_FPS") ?? "60");
        return TimeSpan.FromSeconds(1.0 / targetFps);
    }
}
```

## Error Handling and Logging

### Comprehensive Error Handling

```csharp
public class GameErrorHandler
{
    private readonly ILogger _logger;

    public GameErrorHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        T defaultValue = default)
    {
        try
        {
            return await operation();
        }
        catch (EntityValidationException ex)
        {
            _logger.Error(ex, "Entity validation failed during {Operation}: {Errors}",
                         operationName, string.Join(", ", ex.ValidationErrors));
            return defaultValue;
        }
        catch (EntityLoadingException ex)
        {
            _logger.Error(ex, "Failed to load entity {EntityId} from {Path} during {Operation}",
                         ex.EntityId, ex.EntityPath, operationName);
            return defaultValue;
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Operation {Operation} was cancelled", operationName);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error during {Operation}", operationName);
            return defaultValue;
        }
    }

    public void HandleCriticalError(Exception ex, string context)
    {
        _logger.Fatal(ex, "Critical error in {Context}: {Message}", context, ex.Message);

        // Attempt graceful shutdown
        try
        {
            // Save game state if possible
            SaveEmergencyState();

            // Show error dialog to user
            ShowErrorDialog(ex.Message);
        }
        catch (Exception shutdownEx)
        {
            _logger.Fatal(shutdownEx, "Failed to shutdown gracefully");
        }
        finally
        {
            Environment.Exit(1);
        }
    }

    private void SaveEmergencyState()
    {
        // Implement emergency save logic
        _logger.Information("Attempting emergency state save");
    }

    private void ShowErrorDialog(string message)
    {
        // Implement error dialog display
        Console.WriteLine($"Critical Error: {message}");
    }
}
```

These examples demonstrate common patterns and best practices for Gloam development. Each example includes error handling, logging, and follows the framework's architectural patterns.