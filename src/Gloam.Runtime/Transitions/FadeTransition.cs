using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Core.Transitions;
using Gloam.Runtime.Types;

namespace Gloam.Runtime.Transitions;

/// <summary>
/// Fade transition that fades between scenes through a specific color
/// </summary>
public sealed class FadeTransition : BaseTransition
{
    private readonly Color _fadeColor;
    private readonly FadeDirection _direction;
    private readonly IScene? _sourceScene;
    private readonly IScene _targetScene;

    /// <summary>
    /// Initializes a new fade transition
    /// </summary>
    /// <param name="duration">Duration of the fade effect</param>
    /// <param name="direction">Direction of the fade</param>
    /// <param name="sourceScene">Source scene being faded out</param>
    /// <param name="targetScene">Target scene being faded in</param>
    /// <param name="fadeColor">Color to fade through (default is black)</param>
    public FadeTransition(TimeSpan duration, FadeDirection direction, IScene? sourceScene, IScene targetScene, Color? fadeColor = null)
        : base("Fade", duration)
    {
        _fadeColor = fadeColor ?? Colors.Black;
        _direction = direction;
        _sourceScene = sourceScene;
        _targetScene = targetScene;
    }

    /// <inheritdoc />
    public override async ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        if (Progress <= 0.0f)
        {
            // Render source scene at start
            if (_sourceScene != null)
            {
                await RenderSceneAsync(_sourceScene, context, ct);
            }
            return;
        }

        if (Progress >= 1.0f)
        {
            // Render target scene at end
            await RenderSceneAsync(_targetScene, context, ct);
            return;
        }

        // Calculate alpha based on fade direction
        float alpha = _direction switch
        {
            FadeDirection.FadeIn => GetEasedProgress(EaseInQuad),
            FadeDirection.FadeOut => 1.0f - GetEasedProgress(EaseOutQuad),
            FadeDirection.FadeInOut => Progress <= 0.5f 
                ? GetEasedProgress(t => EaseInQuad(t * 2)) 
                : 1.0f - GetEasedProgress(t => EaseOutQuad((t - 0.5f) * 2)),
            _ => Progress
        };

        // Determine which scene to show based on fade direction and progress
        IScene? sceneToRender = _direction switch
        {
            FadeDirection.FadeIn => _targetScene, // Fading in target scene
            FadeDirection.FadeOut => _sourceScene, // Fading out source scene  
            FadeDirection.FadeInOut => Progress <= 0.5f ? _sourceScene : _targetScene, // Switch at midpoint
            _ => _targetScene
        };

        // Render the appropriate scene first
        if (sceneToRender != null)
        {
            await RenderSceneAsync(sceneToRender, context, ct);
        }

        // Apply fade overlay
        if (alpha > 0.01f) // Only render overlay if visible
        {
            await RenderFadeOverlayAsync(context, alpha, ct);
        }
    }

    /// <summary>
    /// Renders a scene's layers
    /// </summary>
    private static async ValueTask RenderSceneAsync(IScene scene, RenderLayerContext context, CancellationToken ct)
    {
        foreach (var layer in scene.Layers)
        {
            if (layer.IsVisible)
            {
                await layer.RenderAsync(context, ct);
            }
        }
    }

    /// <summary>
    /// Renders the fade overlay efficiently
    /// </summary>
    private async ValueTask RenderFadeOverlayAsync(RenderLayerContext context, float alpha, CancellationToken ct)
    {
        // Create fade color with calculated alpha
        var fadeColorWithAlpha = new Color(
            _fadeColor.R,
            _fadeColor.G,
            _fadeColor.B,
            (byte)(255 * alpha)
        );

        // Efficient full-screen overlay rendering
        var screenHeight = context.Screen.Height;
        var screenWidth = context.Screen.Width;
        
        for (int y = 0; y < screenHeight; y++)
        {
            // Draw a full line at once for better performance
            var line = new string(' ', screenWidth);
            context.Renderer.DrawText(
                new Position(0, y),
                line,
                Colors.Transparent,
                fadeColorWithAlpha
            );
        }

        await ValueTask.CompletedTask;
    }
}