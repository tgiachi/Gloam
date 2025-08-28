using System.Text;
using Gloam.Console.Render.Surfaces;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Rendering;

/// <summary>
///     Console-based renderer that outputs to the system console using ANSI escape codes
/// </summary>
public sealed class ConsoleRenderer : IRenderer
{
    private readonly Dictionary<Position, (char character, ConsoleColor? fg, ConsoleColor? bg)> _drawBuffer;
    private readonly StringBuilder _frameBuffer;
    private readonly ConsoleSurface _surface;
    private bool _isDrawing;

    /// <summary>
    ///     Initializes a new console renderer with the specified surface
    /// </summary>
    /// <param name="surface">The console surface to render to</param>
    public ConsoleRenderer(ConsoleSurface surface)
    {
        _surface = surface ?? throw new ArgumentNullException(nameof(surface));
        _frameBuffer = new StringBuilder();
        _drawBuffer = new Dictionary<Position, (char, ConsoleColor?, ConsoleColor?)>();

        // Initialize console settings
        if (ConsoleSurface.SupportsColor)
        {
            System.Console.OutputEncoding = Encoding.UTF8;
        }
    }

    /// <summary>
    ///     Creates a new console renderer with a default console surface
    /// </summary>
    public ConsoleRenderer() : this(new ConsoleSurface())
    {
    }

    /// <inheritdoc />
    public IRenderSurface Surface => _surface;

    /// <inheritdoc />
    public void BeginDraw()
    {
        if (_isDrawing)
        {
            throw new InvalidOperationException("Already in drawing mode. Call EndDraw() first.");
        }

        _isDrawing = true;
        _drawBuffer.Clear();
        _frameBuffer.Clear();

        // Update surface dimensions in case console was resized
        _surface.UpdateDimensions();

        // Clear screen and position cursor at top-left
        if (ConsoleSurface.SupportsCursorPositioning)
        {
            System.Console.Clear();
        }
    }

    /// <inheritdoc />
    public void DrawText(Position pos, string text, Color fg, Color? bg = null)
    {
        if (!_isDrawing)
        {
            throw new InvalidOperationException("Not in drawing mode. Call BeginDraw() first.");
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (pos.X < 0 || pos.Y < 0 || pos.Y >= _surface.Height)
        {
            return;
        }

        var fgConsole = ConvertToConsoleColor(fg);
        var bgConsole = bg.HasValue && bg.Value.A > 0 ? ConvertToConsoleColor(bg.Value) : (ConsoleColor?)null;

        // Draw each character of the text
        for (var i = 0; i < text.Length; i++)
        {
            var charPos = new Position(pos.X + i, pos.Y);

            if (charPos.X >= _surface.Width)
            {
                break;
            }

            _drawBuffer[charPos] = (text[i], fgConsole, bgConsole);
        }
    }

    /// <inheritdoc />
    public void DrawTile(Position pos, TileVisual v)
    {
        if (!_isDrawing)
        {
            throw new InvalidOperationException("Not in drawing mode. Call BeginDraw() first.");
        }

        if (pos.X < 0 || pos.Y < 0 || pos.X >= _surface.Width || pos.Y >= _surface.Height)
        {
            return;
        }

        var fgConsole = ConvertToConsoleColor(v.Foreground);
        var bgConsole = v.Background.HasValue && v.Background.Value.A > 0
            ? ConvertToConsoleColor(v.Background.Value)
            : (ConsoleColor?)null;

        _drawBuffer[pos] = ((char)v.Glyph.Value, fgConsole, bgConsole);
    }

    /// <inheritdoc />
    public void EndDraw()
    {
        if (!_isDrawing)
        {
            throw new InvalidOperationException("Not in drawing mode. Call BeginDraw() first.");
        }

        try
        {
            RenderFrame();
        }
        finally
        {
            _isDrawing = false;
        }
    }

    private void RenderFrame()
    {
        if (!ConsoleSurface.SupportsCursorPositioning)
        {
            // Fallback for redirected output: render line by line
            RenderToRedirectedOutput();
            return;
        }

        // Render optimized for console with cursor positioning
        foreach (var kvp in _drawBuffer.OrderBy(x => x.Key.Y).ThenBy(x => x.Key.X))
        {
            var pos = kvp.Key;
            var (character, fg, bg) = kvp.Value;

            System.Console.SetCursorPosition(pos.X, pos.Y);

            if (ConsoleSurface.SupportsColor)
            {
                if (fg.HasValue)
                {
                    System.Console.ForegroundColor = fg.Value;
                }

                if (bg.HasValue)
                {
                    System.Console.BackgroundColor = bg.Value;
                }
                else
                {
                    System.Console.BackgroundColor = ConsoleColor.Black; // Reset to default background
                }
            }

            System.Console.Write(character);
        }

        System.Console.ResetColor();
    }

    private void RenderToRedirectedOutput()
    {
        // For redirected output, render a complete frame line by line
        for (var y = 0; y < _surface.Height; y++)
        {
            _frameBuffer.Clear();

            for (var x = 0; x < _surface.Width; x++)
            {
                var pos = new Position(x, y);
                if (_drawBuffer.TryGetValue(pos, out var drawData))
                {
                    _frameBuffer.Append(drawData.character);
                }
                else
                {
                    _frameBuffer.Append(' ');
                }
            }

            System.Console.WriteLine(_frameBuffer.ToString());
        }
    }

    private static ConsoleColor ConvertToConsoleColor(Color color)
    {
        // Simple RGB to ConsoleColor mapping
        // This is a basic implementation - could be enhanced with better color matching
        var (r, g, b) = (color.R, color.G, color.B);

        return (r, g, b) switch
        {
            (0, 0, 0)                                    => ConsoleColor.Black,
            (_, _, _) when r > 200 && g < 100 && b < 100 => ConsoleColor.Red,
            (_, _, _) when r < 100 && g > 200 && b < 100 => ConsoleColor.Green,
            (_, _, _) when r > 200 && g > 200 && b < 100 => ConsoleColor.Yellow,
            (_, _, _) when r < 100 && g < 100 && b > 200 => ConsoleColor.Blue,
            (_, _, _) when r > 200 && g < 100 && b > 200 => ConsoleColor.Magenta,
            (_, _, _) when r < 100 && g > 200 && b > 200 => ConsoleColor.Cyan,
            (_, _, _) when r > 200 && g > 200 && b > 200 => ConsoleColor.White,
            (_, _, _) when r > 100 && g > 100 && b > 100 => ConsoleColor.Gray,
            (_, _, _) when r > 150 && g < 150 && b < 150 => ConsoleColor.DarkRed,
            (_, _, _) when r < 150 && g > 150 && b < 150 => ConsoleColor.DarkGreen,
            (_, _, _) when r > 150 && g > 150 && b < 150 => ConsoleColor.DarkYellow,
            (_, _, _) when r < 150 && g < 150 && b > 150 => ConsoleColor.DarkBlue,
            (_, _, _) when r > 150 && g < 150 && b > 150 => ConsoleColor.DarkMagenta,
            (_, _, _) when r < 150 && g > 150 && b > 150 => ConsoleColor.DarkCyan,
            _                                            => ConsoleColor.DarkGray
        };
    }
}
