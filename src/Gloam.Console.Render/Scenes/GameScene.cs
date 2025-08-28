using Gloam.Console.Render.Layers;
using Gloam.Core.Contexts;
using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Scenes;

/// <summary>
/// Game scene with game-specific layers
/// </summary>
public sealed class GameScene : BaseScene
{
    private ISceneManager? _sceneManager;

    public GameScene() : base("Game")
    {
        // Add game-specific layers in priority order
        AddLayer(new WorldLayer());
        AddLayer(new EntityLayer());
        AddLayer(new GameUILayer(this));
    }

    public void SetSceneManager(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public async ValueTask ReturnToMenuAsync(CancellationToken ct = default)
    {
        if (_sceneManager != null)
        {
            await _sceneManager.SwitchToSceneAsync("MainMenu", ct);
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
    public override int Priority => 20; // Entities render after world
    public override string Name => "Entities";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Draw player character
        var playerX = context.Screen.Width / 2;
        var playerY = context.Screen.Height / 2;

        context.Renderer.DrawText(
            new Position(playerX, playerY),
            "@",
            Colors.PlayerColor,
            Colors.Transparent
        );

        // Draw some example items/enemies
        context.Renderer.DrawText(
            new Position(playerX - 5, playerY - 3),
            "E",
            Colors.EnemyColor,
            Colors.Transparent
        );

        context.Renderer.DrawText(
            new Position(playerX + 7, playerY + 2),
            "$",
            Colors.ItemColor,
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

        // Draw simple HUD
        var hud = "HP: 100/100 | MP: 50/50 | Gold: 250";
        context.Renderer.DrawText(
            new Position(2, context.Screen.Height - 1),
            hud,
            Colors.Green,
            Colors.Transparent
        );

        // Draw legend
        var legend = new[]
        {
            "@ = Player",
            "E = Enemy", 
            "$ = Item",
            "# = Wall",
            ". = Floor",
            "",
            "Press M for Menu"
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

        // Handle input
        if (context.InputDevice.WasPressed(Keys.M))
        {
            await _scene.ReturnToMenuAsync(ct);
        }
    }
}