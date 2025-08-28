namespace Gloam.Core.Interfaces;

/// <summary>
/// Represents a transition between two scenes
/// </summary>
public interface ISceneTransition
{
    /// <summary>
    /// Gets the source scene (the scene being transitioned from)
    /// </summary>
    IScene? SourceScene { get; }
    
    /// <summary>
    /// Gets the target scene (the scene being transitioned to)
    /// </summary>
    IScene TargetScene { get; }
    
    /// <summary>
    /// Gets the transition effect being used
    /// </summary>
    ITransition? Transition { get; }
    
    /// <summary>
    /// Gets whether the transition is currently active
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets whether the transition has completed
    /// </summary>
    bool IsComplete { get; }
    
    /// <summary>
    /// Starts the scene transition
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    ValueTask StartAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Updates the scene transition
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    /// <param name="ct">Cancellation token</param>
    ValueTask UpdateAsync(TimeSpan deltaTime, CancellationToken ct = default);
}