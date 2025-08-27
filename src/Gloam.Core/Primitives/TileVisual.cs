using System.Text;
using Gloam.Core.Types;

namespace Gloam.Core.Primitives;

public readonly record struct TileVisual(
    Rune Glyph,       // es. '#', '·', '＠', '╬'
    Color Foreground, // es. Color.FromRgb(255, 0, 0) (rosso)
    Color? Background = null,
    TextStyle Style = TextStyle.None
)
{
    public static TileVisual FromChar(char ch, Color fg, Color? bg = null, TextStyle style = TextStyle.None)
    {
        return new TileVisual(new Rune(ch), fg, bg, style);
    }

    // Override comodi senza creare costruttori ad hoc
    public TileVisual WithGlyph(Rune g)
    {
        return this with { Glyph = g };
    }

    public TileVisual WithFg(Color fg)
    {
        return this with { Foreground = fg };
    }

    public TileVisual WithBg(Color? bg)
    {
        return this with { Background = bg };
    }

    public TileVisual WithStyle(TextStyle s)
    {
        return this with { Style = s };
    }
}
