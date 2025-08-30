using Gloam.Runtime.Types;

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
    ///     Target frames per second for rendering (default: 60 FPS)
    /// </summary>
    public double TargetFps { get; init; } = 60;

    /// <summary>
    ///     Sleep time between loop iterations when no rendering is needed (default: 5ms)
    /// </summary>
    public TimeSpan SleepTime { get; init; } = TimeSpan.FromMilliseconds(5);

    /// <summary>
    ///     Defines how the game loop should be managed (default: Internal)
    /// </summary>
    public LoopMode LoopMode { get; init; } = LoopMode.Internal;

    /// <summary>
    ///     When LoopMode is External, this defines whether the loop should handle timing internally
    ///     If false, external timing management is expected (default: true)
    /// </summary>
    public bool HandleTimingInExternalMode { get; init; } = true;

    /// <summary>
    ///     Gets the rendering step interval calculated from TargetFps
    /// </summary>
    public TimeSpan RenderStep => TimeSpan.FromMilliseconds(1000.0 / TargetFps);
}
