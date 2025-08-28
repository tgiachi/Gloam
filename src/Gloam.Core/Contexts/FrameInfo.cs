namespace Gloam.Core.Contexts;

/// <summary>
/// Represents timing and performance information for a single frame in the game loop
/// </summary>
/// <param name="FrameNumber">The sequential frame number since the start of the game</param>
/// <param name="DeltaTime">Time elapsed since the last frame was rendered</param>
/// <param name="TotalTime">Total time elapsed since the game started</param>
/// <param name="FramesPerSecond">Current frames per second performance metric</param>
public record FrameInfo(
    long FrameNumber,
    TimeSpan DeltaTime,
    TimeSpan TotalTime,
    float FramesPerSecond
);
