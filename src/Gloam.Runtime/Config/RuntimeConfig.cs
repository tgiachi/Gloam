namespace Gloam.Runtime.Config;

/// <summary>
///     Configuration for runtime game loop timing and behavior.
///     Controls simulation step, rendering step, and frame skipping for different game modes.
/// </summary>
public sealed record RuntimeConfig(
    /// <summary>
    /// Simulation step interval in milliseconds.
    /// Set to 0 for pure turn-based gameplay (simulation only advances on input).
    /// Set to positive value (e.g., 50ms) for real-time gameplay.
    /// </summary>
    double SimulationStep = 0,
    /// <summary>
    /// Rendering step interval in milliseconds.
    /// Controls the maximum frame rate (e.g., 33ms = ~30 FPS, 16ms = ~60 FPS).
    /// </summary>
    double RenderStep = 33,
    /// <summary>
    /// Whether the game loop should skip rendering frames if simulation is behind.
    /// Helps maintain simulation accuracy in real-time games when rendering is slow.
    /// </summary>
    bool AllowFrameSkip = true
);
