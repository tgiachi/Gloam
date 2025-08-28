using Gloam.Core.Contexts;

namespace Gloam.Core.Interfaces.Base;

/// <summary>
/// Base implementation for layer renderers providing common functionality and patterns.
/// Concrete implementations only need to override RenderLayerAsync with their specific rendering logic.
/// </summary>
public abstract class BaseLayerRenderer : ILayerRenderer
{
    /// <summary>
    /// Gets the priority of this layer renderer. Lower values render first.
    /// </summary>
    public abstract int Priority { get; }

    /// <summary>
    /// Gets the name of this layer renderer for debugging and identification purposes.
    /// </summary>
    public abstract string Name { get; }

    public bool IsVisible { get; set; }

    /// <summary>
    /// Renders the layer with the provided context. This is the main method that concrete implementations must override.
    /// </summary>
    /// <param name="context">The rendering context containing renderer, input, and frame information</param>
    /// <param name="ct">Cancellation token to cancel the rendering operation</param>
    public async ValueTask RenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Perform pre-render setup if needed
        await OnPreRenderAsync(context, ct);

        // Execute the main rendering logic
        await RenderLayerAsync(context, ct);

        // Perform post-render cleanup if needed
        await OnPostRenderAsync(context, ct);
    }

    protected BaseLayerRenderer()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Override this method to implement the specific rendering logic for this layer.
    /// </summary>
    /// <param name="context">The rendering context containing renderer, input, and frame information</param>
    /// <param name="ct">Cancellation token to cancel the rendering operation</param>
    protected abstract ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default);

    /// <summary>
    /// Called before the main rendering logic. Override to perform setup operations.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="context">The rendering context</param>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPreRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called after the main rendering logic. Override to perform cleanup operations.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="context">The rendering context</param>
    /// <param name="ct">Cancellation token</param>
    protected virtual ValueTask OnPostRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Helper method to check if this is the first frame being rendered.
    /// </summary>
    /// <param name="context">The rendering context</param>
    /// <returns>True if this is the first frame, false otherwise</returns>
    protected static bool IsFirstFrame(RenderLayerContext context) => context.FrameNumber == 0;

    /// <summary>
    /// Helper method to get the frames per second from the context.
    /// </summary>
    /// <param name="context">The rendering context</param>
    /// <returns>The current frames per second</returns>
    protected static float GetFramesPerSecond(RenderLayerContext context) => context.FrameInfo.FramesPerSecond;
}
