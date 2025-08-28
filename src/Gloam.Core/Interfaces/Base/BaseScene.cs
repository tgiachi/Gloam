using Gloam.Core.Interfaces;

namespace Gloam.Core.Interfaces.Base;

/// <summary>
/// Base implementation for scenes providing common functionality and patterns.
/// Concrete implementations only need to override the lifecycle methods as needed.
/// </summary>
public abstract class BaseScene : IScene
{
    private readonly List<ILayerRenderer> _layers;
    private bool _isActive;

    protected BaseScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Scene name cannot be null or empty.", nameof(name));
        }

        Name = name;
        _layers = new List<ILayerRenderer>();
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IReadOnlyList<ILayerRenderer> Layers => _layers.AsReadOnly();

    /// <inheritdoc />
    public bool IsActive => _isActive;

    /// <inheritdoc />
    public async ValueTask OnActivateAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (_isActive)
        {
            return;
        }

        _isActive = true;

        // Perform pre-activation setup if needed
        await OnPreActivateAsync(ct);

        // Execute the main activation logic
        await ActivateSceneAsync(ct);

        // Perform post-activation setup if needed
        await OnPostActivateAsync(ct);
    }

    /// <inheritdoc />
    public async ValueTask OnDeactivateAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_isActive)
        {
            return;
        }

        // Perform pre-deactivation cleanup if needed
        await OnPreDeactivateAsync(ct);

        // Execute the main deactivation logic
        await DeactivateSceneAsync(ct);

        // Perform post-deactivation cleanup if needed
        await OnPostDeactivateAsync(ct);

        _isActive = false;
    }

    /// <inheritdoc />
    public async ValueTask UpdateAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_isActive)
        {
            return;
        }

        await UpdateSceneAsync(ct);
    }

    /// <summary>
    /// Adds a layer renderer to this scene
    /// </summary>
    /// <param name="layer">The layer renderer to add</param>
    protected void AddLayer(ILayerRenderer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        if (!_layers.Contains(layer))
        {
            _layers.Add(layer);
        }
    }

    /// <summary>
    /// Removes a layer renderer from this scene
    /// </summary>
    /// <param name="layer">The layer renderer to remove</param>
    /// <returns>True if the layer was found and removed, false otherwise</returns>
    protected bool RemoveLayer(ILayerRenderer layer)
    {
        if (layer == null)
        {
            return false;
        }

        return _layers.Remove(layer);
    }

    /// <summary>
    /// Override this method to implement scene-specific activation logic.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask ActivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Override this method to implement scene-specific deactivation logic.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask DeactivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Override this method to implement scene-specific update logic.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask UpdateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called before the main activation logic. Override to perform setup operations.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPreActivateAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called after the main activation logic. Override to perform additional setup.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPostActivateAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called before the main deactivation logic. Override to perform cleanup operations.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPreDeactivateAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called after the main deactivation logic. Override to perform final cleanup.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPostDeactivateAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }
}