using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using ZLinq;

namespace Gloam.Runtime.Services;

/// <summary>
/// Manages the rendering of multiple layers in priority order
/// </summary>
public sealed class LayerRenderingManager : ILayerRenderingManager
{
    private readonly List<ILayerRenderer> _layerRenderers = new();

    /// <summary>
    /// Initializes a new instance with the provided layer renderers, sorted by priority
    /// </summary>
    /// <param name="layerRenderers">Collection of layer renderers to manage</param>
    public LayerRenderingManager()
    {
    }

    /// <summary>
    /// Gets the collection of registered layer renderers in priority order
    /// </summary>
    public IReadOnlyList<ILayerRenderer> LayerRenderers => _layerRenderers.AsReadOnly();

    /// <summary>
    /// Renders all registered layers in priority order
    /// </summary>
    /// <param name="context">The rendering context containing renderer and frame information</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    public async ValueTask RenderAllLayersAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        foreach (var renderer in _layerRenderers.AsValueEnumerable().Where(r => r.IsVisible).ToList())
        {
            await renderer.RenderAsync(context, ct);
        }
    }

    /// <summary>
    /// Adds a new layer renderer and re-sorts the collection by priority
    /// </summary>
    /// <param name="layerRenderer">The layer renderer to add</param>
    public void AddLayerRenderer(ILayerRenderer layerRenderer)
    {
        _layerRenderers.Add(layerRenderer);
        _layerRenderers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    /// <summary>
    /// Removes a layer renderer from the collection
    /// </summary>
    /// <param name="layerRenderer">The layer renderer to remove</param>
    /// <returns>True if the renderer was removed, false if it was not found</returns>
    public bool RemoveLayerRenderer(ILayerRenderer layerRenderer)
    {
        return _layerRenderers.Remove(layerRenderer);
    }
}
