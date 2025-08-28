using Gloam.Core.Primitives;

namespace Gloam.Core.Interfaces;

/// <summary>
///     Interface for rendering graphics to a surface, supporting both text and tile-based rendering
/// </summary>
public interface IRenderer
{
    /// <summary>Gets the render surface this renderer draws to</summary>
    IRenderSurface Surface { get; }

    /// <summary>Begins a drawing frame</summary>
    void BeginDraw();

    /// <summary>Draws text at the specified position with foreground and optional background color</summary>
    /// <param name="pos">Position to draw the text</param>
    /// <param name="text">Text content to draw</param>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Optional background color</param>
    void DrawText(Position pos, string text, Color fg, Color? bg = null);

    /// <summary>Draws a tile visual at the specified position</summary>
    /// <param name="pos">Position to draw the tile</param>
    /// <param name="v">Tile visual data to render</param>
    void DrawTile(Position pos, TileVisual v);

    /// <summary>Ends the drawing frame and presents the rendered content</summary>
    void EndDraw();
}
