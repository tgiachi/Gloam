using Gloam.Console.Render.Layers;
using Gloam.Runtime.Transitions;
using Gloam.Runtime.Types;
using Gloam.Core.Contexts;
using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;

namespace Gloam.Demo.Scenes;

/// <summary>
/// Main menu scene with menu-specific layers
/// </summary>
public sealed class MainMenuScene : BaseScene
{
    private ISceneManager? _sceneManager;

    public MainMenuScene() : base("MainMenu")
    {
        // Add menu-specific layers
        AddLayer(new MenuBackgroundLayer());
        AddLayer(new MenuUILayer(this));
    }

    public void SetSceneManager(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public ValueTask HandleMenuSelectionAsync(int selection, CancellationToken ct = default)
    {
        if (_sceneManager == null) return ValueTask.CompletedTask;

        switch (selection)
        {
            case 1: // Start Game
                var pushTransition = new PushTransition(TimeSpan.FromMilliseconds(1000), PushDirection.FromLeft, 
                    _sceneManager.CurrentScene, _sceneManager.Scenes["Game"]);
                _ = _sceneManager.SwitchToSceneAsync("Game", pushTransition, ct);
                break;
            case 2: // Settings (not implemented yet)
                // Could switch to a SettingsScene when implemented
                break;
            case 3: // Flame Demo
                var fadeTransition = new FadeTransition(TimeSpan.FromMilliseconds(1200), FadeDirection.FadeInOut, 
                    _sceneManager.CurrentScene, _sceneManager.Scenes["Flame"]);
                _ = _sceneManager.SwitchToSceneAsync("Flame", fadeTransition, ct);
                break;
            case 4: // GUI Demo
                var pushTransitionGui = new PushTransition(TimeSpan.FromMilliseconds(800), PushDirection.FromRight, 
                    _sceneManager.CurrentScene, _sceneManager.Scenes["GuiDemo"]);
                _ = _sceneManager.SwitchToSceneAsync("GuiDemo", pushTransitionGui, ct);
                break;
            case 5: // Exit
                Environment.Exit(0);
                break;
        }
        
        return ValueTask.CompletedTask;
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
    private readonly MainMenuScene _scene;

    public MenuUILayer(MainMenuScene scene)
    {
        _scene = scene;
    }

    public override int Priority => 20; // UI renders after background
    public override string Name => "MenuUI";

    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        var centerY = context.Screen.Height / 2;
        var centerX = context.Screen.Width / 2;

        var menuItems = new[]
        {
            "1. Start Game",
            "2. Settings", 
            "3. Flame Demo",
            "4. GUI Demo",
            "5. Exit"
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
        var instructions = "Press ESC to exit, 1-5 to select";
        var instrX = centerX - instructions.Length / 2;
        var instrY = context.Screen.Height - 3;

        context.Renderer.DrawText(
            new Position(instrX, instrY),
            instructions,
            Colors.Gray,
            Colors.Transparent
        );

        // Handle input
        if (context.InputDevice.WasPressed(Keys.D1) || context.InputDevice.WasPressed(Keys.NumPad1))
        {
            await _scene.HandleMenuSelectionAsync(1, ct);
        }
        else if (context.InputDevice.WasPressed(Keys.D2) || context.InputDevice.WasPressed(Keys.NumPad2))
        {
            await _scene.HandleMenuSelectionAsync(2, ct);
        }
        else if (context.InputDevice.WasPressed(Keys.D3) || context.InputDevice.WasPressed(Keys.NumPad3))
        {
            await _scene.HandleMenuSelectionAsync(3, ct);
        }
        else if (context.InputDevice.WasPressed(Keys.D4) || context.InputDevice.WasPressed(Keys.NumPad4))
        {
            await _scene.HandleMenuSelectionAsync(4, ct);
        }
        else if (context.InputDevice.WasPressed(Keys.D5) || context.InputDevice.WasPressed(Keys.NumPad5))
        {
            await _scene.HandleMenuSelectionAsync(5, ct);
        }
    }
}