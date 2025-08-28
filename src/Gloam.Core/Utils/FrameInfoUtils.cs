using System.Diagnostics;
using Gloam.Core.Contexts;

namespace Gloam.Core.Utils;

/// <summary>
///     Utility methods for creating and calculating frame information
/// </summary>
public static class FrameInfoUtils
{
    /// <summary>
    ///     Creates a FrameInfo instance with calculated timing information
    /// </summary>
    /// <param name="startTimestamp">Initial timestamp when the game loop started</param>
    /// <param name="currentTimestamp">Current high-precision timestamp from Stopwatch.GetTimestamp()</param>
    /// <param name="timeSinceLastRender">Time elapsed since the last render</param>
    /// <param name="isFirstFrame">Whether this is the first frame being rendered</param>
    /// <param name="renderStep">The configured render step interval</param>
    /// <returns>A FrameInfo instance with calculated timing data</returns>
    public static FrameInfo Create(
        long startTimestamp, long currentTimestamp, TimeSpan timeSinceLastRender, bool isFirstFrame, TimeSpan renderStep
    )
    {
        var frameNumber = isFirstFrame ? 0 : CalculateFrameNumber(startTimestamp, currentTimestamp, renderStep);
        var totalTime = Stopwatch.GetElapsedTime(startTimestamp, currentTimestamp);
        var framesPerSecond = CalculateFramesPerSecond(timeSinceLastRender, isFirstFrame);

        return new FrameInfo(frameNumber, timeSinceLastRender, totalTime, framesPerSecond);
    }

    /// <summary>
    ///     Calculates the current frame number based on elapsed time and render step
    /// </summary>
    /// <param name="startTimestamp">Initial timestamp when the game loop started</param>
    /// <param name="currentTimestamp">Current high-precision timestamp</param>
    /// <param name="renderStep">The configured render step interval</param>
    /// <returns>The calculated frame number</returns>
    private static long CalculateFrameNumber(long startTimestamp, long currentTimestamp, TimeSpan renderStep)
    {
        var totalTime = Stopwatch.GetElapsedTime(startTimestamp, currentTimestamp);
        return (long)(totalTime.TotalMilliseconds / renderStep.TotalMilliseconds);
    }

    /// <summary>
    ///     Calculates the frames per second based on the time since last render
    /// </summary>
    /// <param name="timeSinceLastRender">Time elapsed since the last render</param>
    /// <param name="isFirstFrame">Whether this is the first frame</param>
    /// <returns>The calculated frames per second</returns>
    private static float CalculateFramesPerSecond(TimeSpan timeSinceLastRender, bool isFirstFrame)
    {
        return isFirstFrame || timeSinceLastRender.TotalMilliseconds <= 0
            ? 0f
            : (float)(1000.0 / timeSinceLastRender.TotalMilliseconds);
    }
}
