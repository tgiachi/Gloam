using Gloam.Core.Primitives;

namespace Gloam.Core.Ui;

/// <summary>
///     Provides rendering capabilities for GUI controls
/// </summary>
public interface IGuiRenderer
{
    /// <summary>
    ///     Renders a filled rectangle
    /// </summary>
    /// <param name="position">The position of the rectangle</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="color">The color to fill with</param>
    void FillRectangle(Position position, Size size, Color color);

    /// <summary>
    ///     Renders the outline of a rectangle
    /// </summary>
    /// <param name="position">The position of the rectangle</param>
    /// <param name="size">The size of the rectangle</param>
    /// <param name="color">The color of the outline</param>
    void DrawRectangle(Position position, Size size, Color color);

    /// <summary>
    ///     Renders text at the specified position
    /// </summary>
    /// <param name="text">The text to render</param>
    /// <param name="position">The position to render at</param>
    /// <param name="color">The color of the text</param>
    void DrawText(string text, Position position, Color color);

    /// <summary>
    ///     Sets the clipping region for rendering
    /// </summary>
    /// <param name="rectangle">The clipping rectangle</param>
    void SetClipRegion(Rectangle rectangle);

    /// <summary>
    ///     Clears the current clipping region
    /// </summary>
    void ClearClipRegion();
}