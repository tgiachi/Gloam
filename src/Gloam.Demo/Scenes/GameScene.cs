
using Gloam.Console.Render.Input;
using Gloam.Runtime.Transitions;
using Gloam.Runtime.Types;
using Gloam.Core.Contexts;
using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;
using Gloam.Core.Types;

namespace Gloam.Demo.Scenes;

/// <summary>
/// Game scene with game-specific layers
/// </summary>
public sealed class GameScene : BaseScene
{
    private ISceneManager? _sceneManager;
    private Position _playerPosition;
    private DateTime _lastMoveTime;
    private readonly TimeSpan _movementCooldown = TimeSpan.FromMilliseconds(150); // Limit to ~6.7 moves per second

    public GameScene() : base("Game")
    {
        // Initialize player at center
        _playerPosition = new Position(40, 12); // Default center position
        _lastMoveTime = DateTime.MinValue; // Allow immediate first move

        // Add game-specific layers in priority order
        AddLayer(new WorldLayer());
        AddLayer(new EntityLayer(this));
        AddLayer(new GameUILayer(this));
    }

    /// <summary>
    /// Gets the current player position
    /// </summary>
    public Position PlayerPosition => _playerPosition;

    public void SetSceneManager(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public async ValueTask ReturnToMenuAsync(CancellationToken ct = default)
    {
        if (_sceneManager != null)
        {
            var pushTransition = new PushTransition(TimeSpan.FromMilliseconds(800), PushDirection.FromRight,
                _sceneManager.CurrentScene, _sceneManager.Scenes["MainMenu"]);
            await _sceneManager.SwitchToSceneAsync("MainMenu", pushTransition, ct);
        }
    }

    protected override ValueTask ActivateSceneAsync(CancellationToken ct = default)
    {
        // Game activation logic here
        return ValueTask.CompletedTask;
    }

    protected override ValueTask DeactivateSceneAsync(CancellationToken ct = default)
    {
        // Game deactivation logic here
        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateSceneAsync(CancellationToken ct = default)
    {
        // Game update logic here (game state, entities, etc.)
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Moves the player to a new position, with bounds checking
    /// </summary>
    /// <param name="newPosition">The new position to move to</param>
    /// <param name="screenWidth">Screen width for bounds checking</param>
    /// <param name="screenHeight">Screen height for bounds checking</param>
    public void MovePlayer(Position newPosition, int screenWidth, int screenHeight)
    {
        // Bounds checking - keep player within the game area (accounting for walls)
        var clampedX = Math.Clamp(newPosition.X, 3, screenWidth - 4);
        var clampedY = Math.Clamp(newPosition.Y, 3, screenHeight - 4);

        _playerPosition = new Position(clampedX, clampedY);
    }

    /// <summary>
    /// Handles player movement input with throttling
    /// </summary>
    /// <param name="inputDevice">Input device to check for movement keys</param>
    /// <param name="screenWidth">Screen width for bounds checking</param>
    /// <param name="screenHeight">Screen height for bounds checking</param>
    public void HandlePlayerInput(IInputDevice inputDevice, int screenWidth, int screenHeight)
    {
        // Check if enough time has passed since last movement
        var now = DateTime.Now;
        if (now - _lastMoveTime < _movementCooldown)
        {
            return; // Still in cooldown, ignore input
        }

        var currentPos = _playerPosition;
        Position newPos = currentPos;

        // Check WASD movement using IsDown for responsive input detection
        if (inputDevice.IsDown(Keys.W) || inputDevice.IsDown(Keys.Up))
        {
            newPos = new Position(currentPos.X, currentPos.Y - 1); // Move up
        }
        else if (inputDevice.IsDown(Keys.S) || inputDevice.IsDown(Keys.Down))
        {
            newPos = new Position(currentPos.X, currentPos.Y + 1); // Move down
        }
        else if (inputDevice.IsDown(Keys.A) || inputDevice.IsDown(Keys.Left))
        {
            newPos = new Position(currentPos.X - 1, currentPos.Y); // Move left
        }
        else if (inputDevice.IsDown(Keys.D) || inputDevice.IsDown(Keys.Right))
        {
            newPos = new Position(currentPos.X + 1, currentPos.Y); // Move right
        }

        // Check mouse input for position selection
        var mouseState = inputDevice.Mouse;
        if (mouseState.Pressed && mouseState.Button == Gloam.Core.Types.MouseButtonType.Left)
        {
            // Convert mouse coordinates to world coordinates (accounting for UI offset)
            var worldX = mouseState.X - 2; // Subtract left margin
            var worldY = mouseState.Y - 2; // Subtract top margin

            // Check if click is within world bounds
            if (worldX >= 0 && worldX < screenWidth - 4 && worldY >= 0 && worldY < screenHeight - 4)
            {
                newPos = new Position(worldX, worldY);
            }
        }

        // Apply movement if position changed
        if (newPos != currentPos)
        {
            MovePlayer(newPos, screenWidth, screenHeight);
            _lastMoveTime = now; // Update last move time
        }
    }
}

/// <summary>
/// World/terrain layer for the game
/// </summary>
internal sealed class WorldLayer : BaseLayerRenderer
{
    public override int Priority => 10; // World renders first
    public override string Name => "World";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Draw simple world/terrain
        for (int y = 2; y < context.Screen.Height - 2; y++)
        {
            for (int x = 2; x < context.Screen.Width - 2; x++)
            {
                char tile;
                Color color;

                // Simple terrain generation
                if (x == 2 || x == context.Screen.Width - 3 || y == 2 || y == context.Screen.Height - 3)
                {
                    tile = '#'; // Walls
                    color = Colors.WallColor;
                }
                else
                {
                    tile = '.'; // Floor
                    color = Colors.FloorColor;
                }

                context.Renderer.DrawText(
                    new Position(x, y),
                    tile.ToString(),
                    color,
                    Colors.Transparent
                );
            }
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Entity layer for game objects
/// </summary>
internal sealed class EntityLayer : BaseLayerRenderer
{
    private readonly GameScene _scene;

    public EntityLayer(GameScene scene)
    {
        _scene = scene;
    }

    public override int Priority => 20; // Entities render after world
    public override string Name => "Entities";

    private Position otherEntityPos = new Position(0, 0);

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        if (otherEntityPos.X == 0)
        {

            otherEntityPos = _scene.PlayerPosition;
        }
        // Draw player character at current position
        var playerPos = _scene.PlayerPosition;

        context.Renderer.DrawText(
            playerPos,
            "@",
            Colors.Orange, // Bright orange player for 24-bit test
            Colors.Transparent
        );

        // Draw some example items/enemies with vibrant colors
        context.Renderer.DrawText(
            new Position(otherEntityPos.X - 5, otherEntityPos.Y - 3),
            "E",
            Colors.DeepPink, // Vibrant pink enemy
            Colors.Transparent
        );

        context.Renderer.DrawText(
            new Position(otherEntityPos.X + 7, otherEntityPos.Y + 2),
            "$",
            Colors.Yellow, // Bright gold item
            Colors.Transparent
        );

        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Game UI layer for HUD elements
/// </summary>
internal sealed class GameUILayer : BaseLayerRenderer
{
    private readonly GameScene _scene;

    public GameUILayer(GameScene scene)
    {
        _scene = scene;
    }

    public override int Priority => 30; // UI renders on top
    public override string Name => "GameUI";

    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Draw game title
        var title = "GLOAM DEMO - GAME SCENE";
        context.Renderer.DrawText(
            new Position(2, 0),
            title,
            Colors.Yellow,
            Colors.Transparent
        );

        // Draw simple HUD with vibrant colors for 24-bit test
        var hudParts = new[]
        {
            ("HP: 100/100", Colors.LightRed),
            (" | ", Colors.Gray),
            ("MP: 50/50", Colors.LightBlue),
            (" | ", Colors.Gray),
            ("Gold: 250", Colors.Orange)
        };

        int hudX = 2;
        for (int i = 0; i < hudParts.Length; i++)
        {
            var (text, color) = hudParts[i];
            context.Renderer.DrawText(
                new Position(hudX, context.Screen.Height - 1),
                text,
                color,
                Colors.Transparent
            );
            hudX += text.Length;
        }

        // Draw legend
        var legend = new[]
        {
            "@ = Player",
            "E = Enemy",
            "$ = Item",
            "# = Wall",
            ". = Floor",
            "",
            "WASD to move",
            "ESC/M for Menu"
        };

        for (int i = 0; i < legend.Length; i++)
        {
            context.Renderer.DrawText(
                new Position(context.Screen.Width - 17, 2 + i),
                legend[i],
                Colors.LightGray,
                Colors.Transparent
            );
        }

        // Handle player movement input
        _scene.HandlePlayerInput(context.InputDevice, context.Screen.Width, context.Screen.Height);



        // Handle menu input
        if (context.InputDevice.WasPressed(Keys.M) || context.InputDevice.WasPressed(Keys.Escape))
        {
            await _scene.ReturnToMenuAsync(ct);
        }
    }
}
