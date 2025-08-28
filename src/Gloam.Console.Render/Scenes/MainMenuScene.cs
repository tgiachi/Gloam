using Gloam.Console.Render.Layers;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Scenes;

/// <summary>
/// Main menu scene with menu-specific layers
/// </summary>
public sealed class MainMenuScene : BaseScene
{
    public MainMenuScene() : base("MainMenu")
    {
        // Add menu-specific layers
        AddLayer(new MenuBackgroundLayer());
        AddLayer(new MenuUILayer());
    }

    protected override ValueTask ActivateSceneAsync(CancellationToken ct = default)
    {
        // Menu activation logic here
        return ValueTask.CompletedTask;
    }

    protected override ValueTask DeactivateSceneAsync(CancellationToken ct = default)
    {
        // Menu deactivation logic here
        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateSceneAsync(CancellationToken ct = default)
    {
        // Menu update logic here (handle menu navigation, etc.)
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Background layer for the main menu
/// </summary>
internal sealed class MenuBackgroundLayer : BaseLayerRenderer
{
    public override int Priority => 10; // Background renders first
    public override string Name => "MenuBackground";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Draw menu background
        var title = "GLOAM DEMO - MAIN MENU";
        var centerX = (context.Screen.Width - title.Length) / 2;
        var centerY = context.Screen.Height / 4;

        context.Renderer.DrawText(
            new Position(centerX, centerY),
            title,
            Colors.Yellow,
            Colors.DarkBlue
        );

        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// UI layer for menu interactions
/// </summary>
internal sealed class MenuUILayer : BaseLayerRenderer
{
    public override int Priority => 20; // UI renders after background
    public override string Name => "MenuUI";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        var centerY = context.Screen.Height / 2;
        var centerX = context.Screen.Width / 2;

        var menuItems = new[]
        {
            "1. Start Game",
            "2. Settings", 
            "3. Exit"
        };

        for (int i = 0; i < menuItems.Length; i++)
        {
            var item = menuItems[i];
            var x = centerX - item.Length / 2;
            var y = centerY + i * 2;

            context.Renderer.DrawText(
                new Position(x, y),
                item,
                Colors.White,
                Colors.Transparent
            );
        }

        // Instructions
        var instructions = "Press ESC to exit, 1-3 to select";
        var instrX = centerX - instructions.Length / 2;
        var instrY = context.Screen.Height - 3;

        context.Renderer.DrawText(
            new Position(instrX, instrY),
            instructions,
            Colors.Gray,
            Colors.Transparent
        );

        return ValueTask.CompletedTask;
    }
}