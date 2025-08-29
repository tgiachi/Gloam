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
/// Flame demo scene showcasing 24-bit color gradients
/// </summary>
public sealed class FlameScene : BaseScene
{
    private ISceneManager? _sceneManager;

    public FlameScene() : base("Flame")
    {
        // Add flame-specific layers
        AddLayer(new FlameBackgroundLayer());
        AddLayer(new FlameAnimationLayer());
        AddLayer(new FlameUILayer(this));
    }

    public void SetSceneManager(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public ValueTask ReturnToMenuAsync(CancellationToken ct = default)
    {
        if (_sceneManager != null)
        {
            var pushTransition = new PushTransition(TimeSpan.FromMilliseconds(600), PushDirection.FromBottom,
                _sceneManager.CurrentScene, _sceneManager.Scenes["MainMenu"]);
            
            // Start transition without blocking - let it run in background
            _ = _sceneManager.SwitchToSceneAsync("MainMenu", pushTransition, ct);
        }
        
        return ValueTask.CompletedTask;
    }

    protected override ValueTask ActivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask DeactivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Background layer for flame scene
/// </summary>
internal sealed class FlameBackgroundLayer : BaseLayerRenderer
{
    public override int Priority => 10;
    public override string Name => "FlameBackground";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Draw dark background to make flames pop
        for (int y = 0; y < context.Screen.Height; y++)
        {
            for (int x = 0; x < context.Screen.Width; x++)
            {
                context.Renderer.DrawText(
                    new Position(x, y),
                    " ",
                    Colors.Black,
                    new Color(10, 10, 20) // Very dark blue background
                );
            }
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Animated flame layer with gradient effects
/// </summary>
internal sealed class FlameAnimationLayer : BaseLayerRenderer
{
    private static int _frameCounter ;

    public override int Priority => 20;
    public override string Name => "FlameAnimation";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        _frameCounter++;

        var centerX = context.Screen.Width / 2;
        var bottomY = context.Screen.Height - 3;

        // Create multiple flame "columns" with different heights and phases
        for (int flameIndex = 0; flameIndex < 5; flameIndex++)
        {
            var flameX = centerX - 8 + (flameIndex * 4);
            var phaseOffset = flameIndex * 20; // Different phase for each flame

            DrawFlameColumn(context, flameX, bottomY, flameIndex, phaseOffset);
        }

        return ValueTask.CompletedTask;
    }

    private static void DrawFlameColumn(RenderLayerContext context, int baseX, int baseY, int flameIndex, int phaseOffset)
    {
        // Calculate flame height with animation (oscillates between 8 and 15)
        var animationValue = Math.Sin((_frameCounter + phaseOffset) * 0.2) * 3.5 + 11.5;
        var flameHeight = (int)animationValue;

        for (int y = 0; y < flameHeight; y++)
        {
            // Calculate intensity based on height (stronger at bottom)
            var intensity = 1.0f - ((float)y / flameHeight);

            // Add some horizontal variance for flame shape
            var variance = (int)(Math.Sin((_frameCounter + phaseOffset + y * 10) * 0.3) * (y * 0.3));

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                var x = baseX + xOffset + variance;
                var currentY = baseY - y;

                if (x >= 0 && x < context.Screen.Width && currentY >= 0 && currentY < context.Screen.Height)
                {
                    // Create gradient from yellow/orange at bottom to red at top
                    var color = GetFlameColor(intensity, xOffset);
                    var character = GetFlameCharacter(intensity, y, _frameCounter + phaseOffset);

                    context.Renderer.DrawText(
                        new Position(x, currentY),
                        character.ToString(),
                        color,
                        Colors.Transparent
                    );
                }
            }
        }
    }

    private static Color GetFlameColor(float intensity, int xOffset)
    {
        // Create gradient from bright yellow/orange at bottom to deep red at top
        var baseIntensity = Math.Max(0.3f, intensity);

        if (intensity > 0.8f)
        {
            // Bottom: Bright yellow/white
            var r = (byte)(255 * baseIntensity);
            var g = (byte)(255 * baseIntensity);
            var b = (byte)(100 * Math.Max(0, intensity - 0.3f));
            return new Color(r, g, b);
        }
        else if (intensity > 0.5f)
        {
            // Middle: Orange
            var r = (byte)(255 * baseIntensity);
            var g = (byte)(180 * baseIntensity);
            var b = (byte)(50 * baseIntensity);
            return new Color(r, g, b);
        }
        else
        {
            // Top: Red
            var r = (byte)(255 * Math.Max(0.4f, baseIntensity));
            var g = (byte)(80 * baseIntensity);
            var b = (byte)(20 * baseIntensity);
            return new Color(r, g, b);
        }
    }

    private static char GetFlameCharacter(float intensity, int height, int frame)
    {
        // Use different characters based on intensity and animation
        var animatedValue = Math.Sin((frame + height * 5) * 0.4);

        if (intensity > 0.8f)
        {
            // Bright flame core
            return animatedValue > 0 ? '█' : '▓';
        }
        else if (intensity > 0.5f)
        {
            // Mid flame
            return animatedValue > 0.3 ? '▒' : '░';
        }
        else
        {
            // Flame tips
            return animatedValue > 0 ? '░' : '·';
        }
    }
}

/// <summary>
/// UI layer for flame scene
/// </summary>
internal sealed class FlameUILayer : BaseLayerRenderer
{
    private readonly FlameScene _scene;

    public FlameUILayer(FlameScene scene)
    {
        _scene = scene;
    }

    public override int Priority => 30;
    public override string Name => "FlameUI";

    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Title
        var title = "FLAME DEMO - 24-BIT COLOR GRADIENTS";
        var titleX = (context.Screen.Width - title.Length) / 2;
        context.Renderer.DrawText(
            new Position(titleX, 1),
            title,
            new Color(255, 200, 100), // Warm gold
            Colors.Transparent
        );

        // Description
        var description = "Showcasing smooth color transitions: Yellow → Orange → Red";
        var descX = (context.Screen.Width - description.Length) / 2;
        context.Renderer.DrawText(
            new Position(descX, 3),
            description,
            Colors.LightGray,
            Colors.Transparent
        );

        // Instructions
        var instructions = "Press ESC/M to return to menu";
        var instrX = (context.Screen.Width - instructions.Length) / 2;
        var instrY = context.Screen.Height - 2;
        context.Renderer.DrawText(
            new Position(instrX, instrY),
            instructions,
            Colors.Gray,
            Colors.Transparent
        );

        // Handle input
        if (context.InputDevice.WasPressed(Keys.M) || context.InputDevice.WasPressed(Keys.Escape))
        {
            _ = _scene.ReturnToMenuAsync(ct);
        }
    }
}
