using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Core.Utils;

namespace Gloam.Core.Contexts;

/// <summary>
///     Provides context information for layer rendering operations including renderer, input, timing, and screen dimensions
/// </summary>
/// <param name="Renderer">The renderer instance for drawing operations</param>
/// <param name="InputDevice">The input device for handling user input</param>
/// <param name="FrameInfo">Current frame timing and performance information</param>
/// <param name="ScreenWidth">Width of the render surface in pixels or cells</param>
/// <param name="ScreenHeight">Height of the render surface in pixels or cells</param>
public sealed record RenderLayerContext(
    IRenderer Renderer,
    IInputDevice InputDevice,
    FrameInfo FrameInfo,
    Size Screen
)
{
    // Convenience properties
    public long FrameNumber => FrameInfo.FrameNumber;
    public TimeSpan DeltaTime => FrameInfo.DeltaTime;
    public TimeSpan TotalTime => FrameInfo.TotalTime;

    /// <summary>
    ///     Creates a RenderLayerContext with calculated frame information
    /// </summary>
    /// <param name="renderer">The renderer to draw with</param>
    /// <param name="inputDevice">The input device for user input</param>
    /// <param name="startTimestamp">Initial timestamp when the game loop started</param>
    /// <param name="currentTimestamp">Current high-precision timestamp from Stopwatch.GetTimestamp()</param>
    /// <param name="timeSinceLastRender">Time elapsed since the last render</param>
    /// <param name="isFirstFrame">Whether this is the first frame being rendered</param>
    /// <param name="renderStep">The configured render step interval</param>
    /// <returns>A RenderLayerContext with calculated timing information</returns>
    public static RenderLayerContext Create(
        IRenderer renderer,
        IInputDevice inputDevice,
        long startTimestamp,
        long currentTimestamp,
        TimeSpan timeSinceLastRender,
        bool isFirstFrame,
        TimeSpan renderStep
    )
    {
        var frameInfo = FrameInfoUtils.Create(
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep
        );

        return new RenderLayerContext(
            renderer,
            inputDevice,
            frameInfo,
            new Size(renderer.Surface.Width, renderer.Surface.Height)
        );
    }
}
