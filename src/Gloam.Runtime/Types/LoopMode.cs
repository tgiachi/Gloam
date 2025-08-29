namespace Gloam.Runtime.Types;

/// <summary>
///     Defines how the game loop should be managed
/// </summary>
public enum LoopMode
{
    /// <summary>
    ///     The game loop is managed internally by the host (default behavior)
    /// </summary>
    Internal,

    /// <summary>
    ///     The game loop is managed externally, allowing manual control over loop execution
    /// </summary>
    External
}