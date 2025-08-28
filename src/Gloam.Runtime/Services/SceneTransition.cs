using Gloam.Core.Interfaces;

namespace Gloam.Runtime.Services;

/// <summary>
/// Implementation of a scene transition
/// </summary>
internal sealed class SceneTransition : ISceneTransition
{
    private bool _isActive;
    private bool _hasStarted;
    
    /// <summary>
    /// Initializes a new scene transition
    /// </summary>
    /// <param name="sourceScene">The source scene (can be null if transitioning from nothing)</param>
    /// <param name="targetScene">The target scene</param>
    /// <param name="transition">The transition effect (can be null for instant transition)</param>
    public SceneTransition(IScene? sourceScene, IScene targetScene, ITransition? transition = null)
    {
        SourceScene = sourceScene;
        TargetScene = targetScene ?? throw new ArgumentNullException(nameof(targetScene));
        Transition = transition;
    }

    /// <inheritdoc />
    public IScene? SourceScene { get; }

    /// <inheritdoc />
    public IScene TargetScene { get; }

    /// <inheritdoc />
    public ITransition? Transition { get; }

    /// <inheritdoc />
    public bool IsActive => _isActive;

    /// <inheritdoc />
    public bool IsComplete => Transition?.IsComplete ?? true;

    /// <inheritdoc />
    public async ValueTask StartAsync(CancellationToken ct = default)
    {
        if (_hasStarted)
        {
            return;
        }

        _hasStarted = true;
        _isActive = true;

        // Start the transition effect if present
        Transition?.Start();

        // If no transition, complete immediately
        if (Transition == null)
        {
            _isActive = false;
        }

        await ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask UpdateAsync(TimeSpan deltaTime, CancellationToken ct = default)
    {
        if (!_isActive || Transition == null)
        {
            return;
        }

        // Update the transition
        Transition.Update(deltaTime);

        // Check if transition completed
        if (Transition.IsComplete)
        {
            _isActive = false;
        }

        await ValueTask.CompletedTask;
    }
}