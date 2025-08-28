namespace Gloam.Runtime.Config;

/// <summary>
///     Configuration for the game loop execution
/// </summary>
public sealed record GameLoopConfig
{
    /// <summary>
    ///     Function that returns true while the game should continue running
    /// </summary>
    public required Func<bool> KeepRunning { get; init; }
    
    /// <summary>
    ///     Fixed time step for game simulation. TimeSpan.Zero for turn-based mode
    /// </summary>
    public TimeSpan SimulationStep { get; init; } = TimeSpan.Zero;
    
    /// <summary>
    ///     Target frame rate for rendering (default: ~30 FPS)
    /// </summary>
    public TimeSpan RenderStep { get; init; } = TimeSpan.FromMilliseconds(33);
    
    /// <summary>
    ///     Maximum sleep time between loop iterations (default: 5ms)
    /// </summary>
    public TimeSpan MaxSleepTime { get; init; } = TimeSpan.FromMilliseconds(5);
}