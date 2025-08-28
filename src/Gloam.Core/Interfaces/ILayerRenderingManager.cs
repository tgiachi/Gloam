using Gloam.Core.Contexts;

namespace Gloam.Core.Interfaces;

/// <summary>
///     Interface for managing the rendering of multiple layers in priority order
/// </summary>
public interface ILayerRenderingManager
{
    /// <summary>
    ///     Gets the collection of registered layer renderers in priority order
    /// </summary>
    IReadOnlyList<ILayerRenderer> LayerRenderers { get; }

    /// <summary>
    ///     Renders all registered layers in priority order
    /// </summary>
    /// <param name="context">The rendering context containing renderer and frame information</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    ValueTask RenderAllLayersAsync(RenderLayerContext context, CancellationToken ct = default);

    /// <summary>
    ///     Adds a new layer renderer and re-sorts the collection by priority
    /// </summary>
    /// <param name="layerRenderer">The layer renderer to add</param>
    void AddLayerRenderer(ILayerRenderer layerRenderer);

    /// <summary>
    ///     Removes a layer renderer from the collection
    /// </summary>
    /// <param name="layerRenderer">The layer renderer to remove</param>
    /// <returns>True if the renderer was removed, false if it was not found</returns>
    bool RemoveLayerRenderer(ILayerRenderer layerRenderer);
}
