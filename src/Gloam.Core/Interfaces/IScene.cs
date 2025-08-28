using Gloam.Core.Interfaces;

namespace Gloam.Core.Interfaces;

/// <summary>
/// Interface for game scenes that manage their own set of layer renderers
/// </summary>
public interface IScene
{
    /// <summary>
    /// Gets the unique name of this scene
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the layer renderers that belong to this scene
    /// </summary>
    IReadOnlyList<ILayerRenderer> Layers { get; }

    /// <summary>
    /// Gets whether this scene is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Called when the scene becomes active
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the activation operation</returns>
    ValueTask OnActivateAsync(CancellationToken ct = default);

    /// <summary>
    /// Called when the scene becomes inactive
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the deactivation operation</returns>
    ValueTask OnDeactivateAsync(CancellationToken ct = default);

    /// <summary>
    /// Called every frame while the scene is active
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the update operation</returns>
    ValueTask UpdateAsync(CancellationToken ct = default);
}