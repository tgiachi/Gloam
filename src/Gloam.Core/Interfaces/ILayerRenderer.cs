using Gloam.Core.Contexts;

namespace Gloam.Core.Interfaces;

/// <summary>
///     Interface for rendering layers in a prioritized order within the game loop
/// </summary>
public interface ILayerRenderer
{
    /// <summary>
    ///     Gets the priority of this layer renderer. Lower values render first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Gets the name of this layer renderer for debugging and identification purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets or sets whether this layer is visible and should be rendered
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    ///     Renders the layer with the provided context
    /// </summary>
    /// <param name="context">The rendering context containing renderer, input, and frame information</param>
    /// <param name="ct">Cancellation token to cancel the rendering operation</param>
    /// <returns>A ValueTask representing the asynchronous rendering operation</returns>
    ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default);
}
