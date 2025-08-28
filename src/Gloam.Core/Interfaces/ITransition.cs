using Gloam.Core.Contexts;

namespace Gloam.Core.Interfaces;

/// <summary>
/// Represents a visual transition effect between scenes
/// </summary>
public interface ITransition
{
    /// <summary>
    /// Gets the name of the transition
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the total duration of the transition
    /// </summary>
    TimeSpan Duration { get; }
    
    /// <summary>
    /// Gets whether the transition has completed
    /// </summary>
    bool IsComplete { get; }
    
    /// <summary>
    /// Gets the current progress of the transition (0.0 to 1.0)
    /// </summary>
    float Progress { get; }
    
    /// <summary>
    /// Starts the transition
    /// </summary>
    void Start();
    
    /// <summary>
    /// Updates the transition with the elapsed time
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    void Update(TimeSpan deltaTime);
    
    /// <summary>
    /// Renders the transition effect
    /// </summary>
    /// <param name="context">The render layer context</param>
    /// <param name="ct">Cancellation token</param>
    ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default);
    
    /// <summary>
    /// Resets the transition to its initial state
    /// </summary>
    void Reset();
}