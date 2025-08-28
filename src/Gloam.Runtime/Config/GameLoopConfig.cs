namespace Gloam.Runtime.Config;

/// <summary>
///     Configuration for the roguelike game loop execution (input-driven, real-time rendering)
/// </summary>
public sealed record GameLoopConfig
{
    /// <summary>
    ///     Function that returns true while the game should continue running
    /// </summary>
    public required Func<bool> KeepRunning { get; init; }

    /// <summary>
    ///     Target frame rate for rendering (default: ~30 FPS)
    /// </summary>
    public TimeSpan RenderStep { get; init; } = TimeSpan.FromMilliseconds(33);

    /// <summary>
    ///     Sleep time between loop iterations when no rendering is needed (default: 5ms)
    /// </summary>
    public TimeSpan SleepTime { get; init; } = TimeSpan.FromMilliseconds(5);
}
