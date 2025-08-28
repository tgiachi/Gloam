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
    private readonly Dictionary<Position, (char character, Color? fg, Color? bg)> _drawBuffer;
    private readonly Dictionary<Position, (char character, Color? fg, Color? bg)> _previousBuffer;
    private readonly StringBuilder _frameBuffer;
    private readonly ConsoleSurface _surface;
    private bool _isDrawing;
    private bool _isFirstFrame;
    private static readonly bool _supports24BitColor = DetectTrueColorSupport();

    /// <summary>
    ///     Initializes a new console renderer with the specified surface
    /// </summary>
    /// <param name="surface">The console surface to render to</param>
    public ConsoleRenderer(ConsoleSurface surface)
    {
        _surface = surface ?? throw new ArgumentNullException(nameof(surface));
        _frameBuffer = new StringBuilder();
        _drawBuffer = new Dictionary<Position, (char, Color?, Color?)>();
        _previousBuffer = new Dictionary<Position, (char, Color?, Color?)>();
        _isFirstFrame = true;

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

        // Clear screen only on first frame or if console was resized
        if (ConsoleSurface.SupportsCursorPositioning && _isFirstFrame)
        {
            System.Console.Clear();
            _isFirstFrame = false;
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

        // Store original colors - conversion happens at render time
        var fgColor = fg;
        var bgColor = bg.HasValue && bg.Value.A > 0 ? bg.Value : (Color?)null;

        // Draw each character of the text
        for (var i = 0; i < text.Length; i++)
        {
            var charPos = new Position(pos.X + i, pos.Y);

            if (charPos.X >= _surface.Width)
            {
                break;
            }

            _drawBuffer[charPos] = (text[i], fgColor, bgColor);
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

        // Store original colors - conversion happens at render time
        var fgColor = v.Foreground;
        var bgColor = v.Background.HasValue && v.Background.Value.A > 0 ? v.Background.Value : (Color?)null;

        _drawBuffer[pos] = ((char)v.Glyph.Value, fgColor, bgColor);
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

        // Double buffering: only update changed positions
        var positionsToUpdate = new List<(Position pos, (char character, Color? fg, Color? bg) data)>();

        // Find positions that changed
        foreach (var kvp in _drawBuffer)
        {
            var pos = kvp.Key;
            var newData = kvp.Value;

            if (!_previousBuffer.TryGetValue(pos, out var oldData) || !oldData.Equals(newData))
            {
                positionsToUpdate.Add((pos, newData));
            }
        }

        // Find positions that were cleared (existed in previous but not in current)
        foreach (var kvp in _previousBuffer)
        {
            var pos = kvp.Key;
            if (!_drawBuffer.ContainsKey(pos))
            {
                // Clear this position with a space
                positionsToUpdate.Add((pos, (' ', Colors.Gray, Colors.Black)));
            }
        }

        // Sort by position for optimal cursor movement
        positionsToUpdate.Sort((a, b) =>
        {
            var yCompare = a.pos.Y.CompareTo(b.pos.Y);
            return yCompare != 0 ? yCompare : a.pos.X.CompareTo(b.pos.X);
        });

        // Update only changed positions
        foreach (var (pos, (character, fg, bg)) in positionsToUpdate)
        {
            System.Console.SetCursorPosition(pos.X, pos.Y);

            if (ConsoleSurface.SupportsColor)
            {
                if (_supports24BitColor)
                {
                    // Use 24-bit ANSI escape sequences
                    var colorSequence = new StringBuilder();

                    if (fg.HasValue)
                    {
                        colorSequence.Append(GetAnsi24BitForeground(fg.Value));
                    }

                    if (bg.HasValue)
                    {
                        colorSequence.Append(GetAnsi24BitBackground(bg.Value));
                    }
                    else
                    {
                        colorSequence.Append(GetAnsi24BitBackground(Colors.Black));
                    }

                    // Write the ANSI sequence + character + reset
                    System.Console.Write(colorSequence.ToString() + character + AnsiReset);
                }
                else
                {
                    // Fallback to legacy ConsoleColor
                    if (fg.HasValue)
                    {
                        System.Console.ForegroundColor = ConvertToConsoleColor(fg.Value);
                    }

                    if (bg.HasValue)
                    {
                        System.Console.BackgroundColor = ConvertToConsoleColor(bg.Value);
                    }
                    else
                    {
                        System.Console.BackgroundColor = ConsoleColor.Black;
                    }

                    System.Console.Write(character);
                }
            }
            else
            {
                // No color support
                System.Console.Write(character);
            }
        }

        // Reset color only for legacy mode (24-bit mode uses ANSI reset in each character)
        if (!_supports24BitColor && ConsoleSurface.SupportsColor)
        {
            System.Console.ResetColor();
        }

        // Copy current buffer to previous buffer for next frame
        _previousBuffer.Clear();
        foreach (var kvp in _drawBuffer)
        {
            _previousBuffer[kvp.Key] = kvp.Value;
        }
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

    /// <summary>
    /// Detects if the current terminal supports 24-bit True Color
    /// </summary>
    /// <returns>True if 24-bit color is supported</returns>
    private static bool DetectTrueColorSupport()
    {
        // Check common environment variables that indicate 24-bit color support
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (!string.IsNullOrEmpty(colorTerm))
        {
            var colorTermLower = colorTerm.ToLowerInvariant();
            if (colorTermLower.Contains("truecolor") || colorTermLower.Contains("24bit"))
            {
                return true;
            }
        }

        // Check TERM variable for terminals known to support 24-bit color
        var term = Environment.GetEnvironmentVariable("TERM");
        if (!string.IsNullOrEmpty(term))
        {
            var termLower = term.ToLowerInvariant();
            return termLower.Contains("256color") ||
                   termLower.Contains("truecolor") ||
                   termLower.StartsWith("xterm-") ||
                   termLower.StartsWith("screen-") ||
                   termLower == "tmux" ||
                   termLower == "alacritty" ||
                   termLower == "kitty";
        }

        // On Windows, modern Windows Terminal and Windows 10+ console support 24-bit
        if (OperatingSystem.IsWindows())
        {
            return Environment.OSVersion.Version.Major >= 10;
        }

        // Default to false for unknown terminals
        return false;
    }

    /// <summary>
    /// Generates ANSI escape sequence for 24-bit foreground color
    /// </summary>
    /// <param name="color">Color to convert</param>
    /// <returns>ANSI escape sequence</returns>
    private static string GetAnsi24BitForeground(Color color)
    {
        return $"\e[38;2;{color.R};{color.G};{color.B}m";
    }

    /// <summary>
    /// Generates ANSI escape sequence for 24-bit background color
    /// </summary>
    /// <param name="color">Color to convert</param>
    /// <returns>ANSI escape sequence</returns>
    private static string GetAnsi24BitBackground(Color color)
    {
        return $"\e[48;2;{color.R};{color.G};{color.B}m";
    }

    /// <summary>
    /// ANSI reset sequence to clear all formatting
    /// </summary>
    private const string AnsiReset = "\e[0m";

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
