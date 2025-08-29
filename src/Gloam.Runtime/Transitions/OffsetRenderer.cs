using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;

namespace Gloam.Runtime.Transitions;

/// <summary>
/// Wrapper renderer that applies position offsets to all draw operations
/// </summary>
internal sealed class OffsetRenderer : IRenderer
{
    private readonly IRenderer _innerRenderer;
    private readonly int _offsetX;
    private readonly int _offsetY;

    /// <summary>
    /// Initializes a new offset renderer
    /// </summary>
    /// <param name="innerRenderer">The renderer to wrap</param>
    /// <param name="offsetX">X offset to apply to all positions</param>
    /// <param name="offsetY">Y offset to apply to all positions</param>
    public OffsetRenderer(IRenderer innerRenderer, int offsetX, int offsetY)
    {
        _innerRenderer = innerRenderer ?? throw new ArgumentNullException(nameof(innerRenderer));
        _offsetX = offsetX;
        _offsetY = offsetY;
    }

    /// <inheritdoc />
    public IRenderSurface Surface => _innerRenderer.Surface;

    /// <inheritdoc />
    public void BeginDraw()
    {
        _innerRenderer.BeginDraw();
    }

    /// <inheritdoc />
    public void EndDraw()
    {
        _innerRenderer.EndDraw();
    }

    /// <inheritdoc />
    public void DrawText(Position pos, string text, Color fg, Color? bg = null)
    {
        var offsetPos = new Position(pos.X + _offsetX, pos.Y + _offsetY);
        
        // Only draw if the offset position is within the surface bounds
        if (offsetPos.X >= 0 && offsetPos.Y >= 0 && 
            offsetPos.X < Surface.Width && offsetPos.Y < Surface.Height)
        {
            _innerRenderer.DrawText(offsetPos, text, fg, bg);
        }
    }

    /// <inheritdoc />
    public void DrawTile(Position pos, TileVisual visual)
    {
        var offsetPos = new Position(pos.X + _offsetX, pos.Y + _offsetY);
        
        // Only draw if the offset position is within the surface bounds
        if (offsetPos.X >= 0 && offsetPos.Y >= 0 && 
            offsetPos.X < Surface.Width && offsetPos.Y < Surface.Height)
        {
            _innerRenderer.DrawTile(offsetPos, visual);
        }
    }
}