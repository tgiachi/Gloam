using System.Text;
using Gloam.Core.Types;

namespace Gloam.Core.Primitives;

/// <summary>
///     Represents a visual tile with glyph, colors, and text styling for tile-based rendering
/// </summary>
/// <param name="Glyph">The character or unicode symbol to display (e.g., '#', '·', '＠', '╬')</param>
/// <param name="Foreground">The foreground color for the glyph</param>
/// <param name="Background">Optional background color (null for transparent)</param>
/// <param name="Style">Text styling flags (bold, underline, etc.)</param>
public readonly record struct TileVisual(
    Rune Glyph,
    Color Foreground,
    Color? Background = null,
    TextStyle Style = TextStyle.None
)
{
    /// <summary>
    ///     Creates a TileVisual from a character with specified colors and styling
    /// </summary>
    /// <param name="ch">Character to display</param>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Optional background color</param>
    /// <param name="style">Optional text styling</param>
    /// <returns>A new TileVisual instance</returns>
    public static TileVisual FromChar(char ch, Color fg, Color? bg = null, TextStyle style = TextStyle.None)
    {
        return new TileVisual(new Rune(ch), fg, bg, style);
    }

    /// <summary>
    ///     Creates a copy of this TileVisual with a different glyph
    /// </summary>
    /// <param name="g">The new glyph to use</param>
    /// <returns>A new TileVisual with the specified glyph</returns>
    public TileVisual WithGlyph(Rune g)
    {
        return this with { Glyph = g };
    }

    /// <summary>
    ///     Creates a copy of this TileVisual with a different foreground color
    /// </summary>
    /// <param name="fg">The new foreground color</param>
    /// <returns>A new TileVisual with the specified foreground color</returns>
    public TileVisual WithFg(Color fg)
    {
        return this with { Foreground = fg };
    }

    /// <summary>
    ///     Creates a copy of this TileVisual with a different background color
    /// </summary>
    /// <param name="bg">The new background color (or null for transparent)</param>
    /// <returns>A new TileVisual with the specified background color</returns>
    public TileVisual WithBg(Color? bg)
    {
        return this with { Background = bg };
    }

    /// <summary>
    ///     Creates a copy of this TileVisual with different text styling
    /// </summary>
    /// <param name="s">The new text style flags</param>
    /// <returns>A new TileVisual with the specified style</returns>
    public TileVisual WithStyle(TextStyle s)
    {
        return this with { Style = s };
    }
}
