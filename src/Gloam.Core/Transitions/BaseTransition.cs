using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;

namespace Gloam.Core.Transitions;

/// <summary>
/// Base class for scene transitions
/// </summary>
public abstract class BaseTransition : ITransition
{
    private TimeSpan _elapsedTime;
    private bool _isStarted;

    /// <summary>
    /// Initializes a new transition with the specified duration
    /// </summary>
    /// <param name="name">The name of the transition</param>
    /// <param name="duration">The duration of the transition</param>
    protected BaseTransition(string name, TimeSpan duration)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Duration = duration;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public TimeSpan Duration { get; }

    /// <inheritdoc />
    public bool IsComplete => _elapsedTime >= Duration;

    /// <inheritdoc />
    public float Progress => Duration.Ticks == 0 ? 1.0f : Math.Clamp((float)(_elapsedTime.Ticks / (double)Duration.Ticks), 0.0f, 1.0f);

    /// <inheritdoc />
    public virtual void Start()
    {
        _isStarted = true;
        _elapsedTime = TimeSpan.Zero;
    }

    /// <inheritdoc />
    public virtual void Update(TimeSpan deltaTime)
    {
        if (!_isStarted)
        {
            return;
        }

        _elapsedTime += deltaTime;
    }

    /// <inheritdoc />
    public abstract ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default);

    /// <inheritdoc />
    public virtual void Reset()
    {
        _isStarted = false;
        _elapsedTime = TimeSpan.Zero;
    }

    /// <summary>
    /// Gets an eased progress value using the specified easing function
    /// </summary>
    /// <param name="easingFunction">The easing function to apply</param>
    /// <returns>The eased progress value</returns>
    protected float GetEasedProgress(Func<float, float> easingFunction)
    {
        return easingFunction(Progress);
    }

    /// <summary>
    /// Linear interpolation between two values
    /// </summary>
    /// <param name="start">Start value</param>
    /// <param name="end">End value</param>
    /// <param name="t">Interpolation factor (0.0 to 1.0)</param>
    /// <returns>Interpolated value</returns>
    protected static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    /// <summary>
    /// Ease-in quadratic function
    /// </summary>
    /// <param name="t">Input value (0.0 to 1.0)</param>
    /// <returns>Eased value</returns>
    protected static float EaseInQuad(float t) => t * t;

    /// <summary>
    /// Ease-out quadratic function
    /// </summary>
    /// <param name="t">Input value (0.0 to 1.0)</param>
    /// <returns>Eased value</returns>
    protected static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);

    /// <summary>
    /// Ease-in-out quadratic function
    /// </summary>
    /// <param name="t">Input value (0.0 to 1.0)</param>
    /// <returns>Eased value</returns>
    protected static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - 2 * (1 - t) * (1 - t);
}