using System.Globalization;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Core.Transitions;
using Gloam.Runtime.Types;

namespace Gloam.Runtime.Transitions;

/// <summary>
/// Push transition where new scene slides in from a direction, pushing the old scene out
/// </summary>
public sealed class PushTransition : BaseTransition
{
    private readonly PushDirection _direction;
    private readonly IScene? _sourceScene;
    private readonly IScene _targetScene;

    /// <summary>
    /// Initializes a new push transition
    /// </summary>
    /// <param name="duration">Duration of the push effect</param>
    /// <param name="direction">Direction to push from</param>
    /// <param name="sourceScene">Source scene being pushed out</param>
    /// <param name="targetScene">Target scene pushing in</param>
    public PushTransition(TimeSpan duration, PushDirection direction, IScene? sourceScene, IScene targetScene)
        : base("Push", duration)
    {
        _direction = direction;
        _sourceScene = sourceScene;
        _targetScene = targetScene;
    }

    /// <inheritdoc />
    public override async ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        if (Progress <= 0.0f)
        {
            return;
        }

        var screenWidth = context.Screen.Width;
        var screenHeight = context.Screen.Height;
        
        // Get eased progress for smooth animation
        var easedProgress = GetEasedProgress(EaseInOutQuad);

        // Calculate offsets based on direction
        var (sourceOffsetX, sourceOffsetY, targetOffsetX, targetOffsetY) = _direction switch
        {
            PushDirection.FromLeft => (
                (int)(easedProgress * screenWidth),     // Source moves right
                0,
                (int)((easedProgress - 1) * screenWidth), // Target comes from left
                0
            ),
            PushDirection.FromRight => (
                (int)(-easedProgress * screenWidth),    // Source moves left
                0,
                (int)((1 - easedProgress) * screenWidth), // Target comes from right
                0
            ),
            PushDirection.FromTop => (
                0,
                (int)(easedProgress * screenHeight),    // Source moves down
                0,
                (int)((easedProgress - 1) * screenHeight) // Target comes from top
            ),
            PushDirection.FromBottom => (
                0,
                (int)(-easedProgress * screenHeight),   // Source moves up
                0,
                (int)((1 - easedProgress) * screenHeight) // Target comes from bottom
            ),
            _ => (0, 0, 0, 0)
        };

        // Clear screen with a transition indicator
        var transitionChar = _direction switch
        {
            PushDirection.FromLeft => '◄',
            PushDirection.FromRight => '►',
            PushDirection.FromTop => '▲',
            PushDirection.FromBottom => '▼',
            _ => '■'
        };

        // Draw transition effect with visible characters showing the push direction
        for (int y = 0; y < screenHeight; y++)
        {
            for (int x = 0; x < screenWidth; x++)
            {
                var pos = new Position(x, y);

                // Create a visual representation of the push
                var color = Colors.Gray;
                var bgColor = Colors.Black;
                
                // Show progress with different characters/colors
                if (_direction == PushDirection.FromLeft)
                {
                    if (x < (int)(easedProgress * screenWidth))
                    {
                        // Area where new scene should be
                        color = Colors.Green;
                        bgColor = Colors.DarkGreen;
                        context.Renderer.DrawText(pos, "→", color, bgColor);
                    }
                    else
                    {
                        // Area where old scene is being pushed out
                        color = Colors.Red;
                        bgColor = Colors.DarkRed;
                        context.Renderer.DrawText(pos, "←", color, bgColor);
                    }
                }
                else if (_direction == PushDirection.FromRight)
                {
                    if (x > screenWidth - (int)(easedProgress * screenWidth))
                    {
                        // Area where new scene should be
                        color = Colors.Green;
                        bgColor = Colors.DarkGreen;
                        context.Renderer.DrawText(pos, "←", color, bgColor);
                    }
                    else
                    {
                        // Area where old scene is being pushed out
                        color = Colors.Red;
                        bgColor = Colors.DarkRed;
                        context.Renderer.DrawText(pos, "→", color, bgColor);
                    }
                }
                else if (_direction == PushDirection.FromTop)
                {
                    if (y < (int)(easedProgress * screenHeight))
                    {
                        // Area where new scene should be
                        color = Colors.Green;
                        bgColor = Colors.DarkGreen;
                        context.Renderer.DrawText(pos, "↓", color, bgColor);
                    }
                    else
                    {
                        // Area where old scene is being pushed out
                        color = Colors.Red;
                        bgColor = Colors.DarkRed;
                        context.Renderer.DrawText(pos, "↑", color, bgColor);
                    }
                }
                else if (_direction == PushDirection.FromBottom)
                {
                    if (y > screenHeight - (int)(easedProgress * screenHeight))
                    {
                        // Area where new scene should be
                        color = Colors.Green;
                        bgColor = Colors.DarkGreen;
                        context.Renderer.DrawText(pos, "↑", color, bgColor);
                    }
                    else
                    {
                        // Area where old scene is being pushed out
                        color = Colors.Red;
                        bgColor = Colors.DarkRed;
                        context.Renderer.DrawText(pos, "↓", color, bgColor);
                    }
                }
            }
        }

        // Add a progress indicator in the center
        var centerX = screenWidth / 2;
        var centerY = screenHeight / 2;
        var progressText = $"PUSH {_direction.ToString().ToUpper(CultureInfo.InvariantCulture)} {(int)(Progress * 100)}%";
        var textX = Math.Max(0, centerX - progressText.Length / 2);
        
        context.Renderer.DrawText(
            new Position(textX, centerY), 
            progressText,
            Colors.Yellow,
            Colors.Black
        );

        await ValueTask.CompletedTask;
    }
}