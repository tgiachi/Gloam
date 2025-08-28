using Gloam.Core.Contexts;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Layers;

/// <summary>
///     Example layer renderer that displays status information
/// </summary>
public sealed class StatusLayerRenderer : BaseLayerRenderer
{
    private readonly Color? _backgroundColor;
    private readonly Color _textColor;

    /// <summary>
    ///     Initializes a new status layer renderer
    /// </summary>
    /// <param name="textColor">Color for status text</param>
    /// <param name="backgroundColor">Optional background color for status text</param>
    public StatusLayerRenderer(Color? textColor = null, Color? backgroundColor = null)
    {
        _textColor = textColor ?? new Color(255, 255, 0);
        _backgroundColor = backgroundColor;
    }

    /// <inheritdoc />
    public override int Priority => 100; // Status renders on top

    /// <inheritdoc />
    public override string Name => "Status";

    /// <inheritdoc />
    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Display frame information
        var fpsText = $"FPS: {context.FrameInfo.FramesPerSecond:F1}";
        var frameText = $"Frame: {context.FrameNumber}";
        var timeText = $"Time: {context.TotalTime.TotalSeconds:F1}s";

        // Draw status line at the top-right corner
        var statusY = 1; // Just below the border

        DrawStatusText(context, fpsText, context.Screen.Width - fpsText.Length - 2, statusY, Colors.Green);
        DrawStatusText(context, frameText, context.Screen.Width - frameText.Length - 2, statusY + 1, Colors.Yellow);
        DrawStatusText(context, timeText, context.Screen.Width - timeText.Length - 2, statusY + 2, Colors.Cyan);

        // Display input instructions at the bottom
        var instructionsY = context.Screen.Height - 2; // Just above the border
        var instructions = "Press ESC to exit";

        DrawStatusText(context, instructions, 2, instructionsY);

        return ValueTask.CompletedTask;
    }

    private void DrawStatusText(
        RenderLayerContext context, string text, int x, int y, Color? foreground = null, Color? background = null
    )
    {
        if (x < 0 || y < 0 || y >= context.Screen.Height)
        {
            return;
        }

        var pos = new Position(x, y);
        context.Renderer.DrawText(pos, text, foreground ?? _textColor, background ?? _backgroundColor);
    }
}
