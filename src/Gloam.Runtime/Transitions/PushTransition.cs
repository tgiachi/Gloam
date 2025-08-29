using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
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
            // Render source scene layers at their normal position
            if (_sourceScene != null)
            {
                foreach (var layer in _sourceScene.Layers)
                {
                    if (layer.IsVisible)
                    {
                        await layer.RenderAsync(context, ct);
                    }
                }
            }
            return;
        }

        if (Progress >= 1.0f)
        {
            // Render target scene layers at their normal position
            foreach (var layer in _targetScene.Layers)
            {
                if (layer.IsVisible)
                {
                    await layer.RenderAsync(context, ct);
                }
            }
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

        // For now, create a temporary wrapper renderer that applies offsets
        var sourceRenderer = new OffsetRenderer(context.Renderer, sourceOffsetX, sourceOffsetY);
        var targetRenderer = new OffsetRenderer(context.Renderer, targetOffsetX, targetOffsetY);
        
        var sourceContext = context with { Renderer = sourceRenderer };
        var targetContext = context with { Renderer = targetRenderer };

        // Render source scene layers with offset (being pushed out)
        if (_sourceScene != null)
        {
            foreach (var layer in _sourceScene.Layers)
            {
                if (layer.IsVisible)
                {
                    await layer.RenderAsync(sourceContext, ct);
                }
            }
        }

        // Render target scene layers with offset (pushing in)
        foreach (var layer in _targetScene.Layers)
        {
            if (layer.IsVisible)
            {
                await layer.RenderAsync(targetContext, ct);
            }
        }
    }
}
