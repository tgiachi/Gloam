using Gloam.Core.Contexts;
using Gloam.Core.Primitives;
using Gloam.Core.Transitions;
using Gloam.Runtime.Types;

namespace Gloam.Runtime.Transitions;

/// <summary>
/// Fade transition that fades to a specific color and back
/// </summary>
public sealed class FadeTransition : BaseTransition
{
    private readonly Color _fadeColor;
    private readonly FadeDirection _direction;

    /// <summary>
    /// Initializes a new fade transition
    /// </summary>
    /// <param name="duration">Duration of the fade effect</param>
    /// <param name="fadeColor">Color to fade to (default is black)</param>
    /// <param name="direction">Direction of the fade (default is FadeOut)</param>
    public FadeTransition(TimeSpan duration, Color? fadeColor = null, FadeDirection direction = FadeDirection.FadeOut)
        : base("Fade", duration)
    {
        _fadeColor = fadeColor ?? Colors.Black;
        _direction = direction;
    }

    /// <inheritdoc />
    public override async ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        if (Progress <= 0.0f)
        {
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

        // Create fade color with calculated alpha
        var fadeColorWithAlpha = new Color(
            _fadeColor.R,
            _fadeColor.G,
            _fadeColor.B,
            (byte)(255 * alpha)
        );

        // Fill the entire screen with the fade color
        for (int y = 0; y < context.Screen.Height; y++)
        {
            for (int x = 0; x < context.Screen.Width; x++)
            {
                // Only draw if alpha is significant enough to be visible
                if (alpha > 0.1f)
                {
                    context.Renderer.DrawText(
                        new Position(x, y),
                        " ",
                        Colors.Transparent,
                        fadeColorWithAlpha
                    );
                }
            }
        }

        await ValueTask.CompletedTask;
    }
}