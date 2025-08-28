namespace Gloam.Console.Render.Types;

/// <summary>
/// Fade direction enumeration for scene transitions
/// </summary>
public enum FadeDirection
{
    /// <summary>
    /// Fade from transparent to opaque
    /// </summary>
    FadeIn,

    /// <summary>
    /// Fade from opaque to transparent
    /// </summary>
    FadeOut,

    /// <summary>
    /// Fade in and then fade out
    /// </summary>
    FadeInOut
}