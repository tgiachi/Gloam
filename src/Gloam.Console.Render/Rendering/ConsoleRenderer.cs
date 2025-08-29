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
    private CellData[] _drawBuffer;
    private CellData[] _previousBuffer;
    private int _bufferWidth;
    private int _bufferHeight;
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
        _bufferWidth = _surface.Width;
        _bufferHeight = _surface.Height;
        var bufferSize = _bufferWidth * _bufferHeight;
        _drawBuffer = new CellData[bufferSize];
        _previousBuffer = new CellData[bufferSize];
        
        // Initialize with empty cells
        for (var i = 0; i < bufferSize; i++)
        {
            _drawBuffer[i] = CellData.Empty;
            _previousBuffer[i] = CellData.Empty;
        }
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
        // Clear draw buffer by setting all cells to empty
        for (var i = 0; i < _drawBuffer.Length; i++)
        {
            _drawBuffer[i] = CellData.Empty;
        }
        _frameBuffer.Clear();

        // Update surface dimensions in case console was resized
        _surface.UpdateDimensions();
        
        // Resize buffers if needed
        if (_bufferWidth != _surface.Width || _bufferHeight != _surface.Height)
        {
            ResizeBuffers(_surface.Width, _surface.Height);
        }

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

            var index = charPos.Y * _bufferWidth + charPos.X;
            _drawBuffer[index] = new CellData(text[i], fgColor, bgColor);
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

        var index = pos.Y * _bufferWidth + pos.X;
        _drawBuffer[index] = new CellData((char)v.Glyph.Value, fgColor, bgColor);
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

    private void ResizeBuffers(int newWidth, int newHeight)
    {
        _bufferWidth = newWidth;
        _bufferHeight = newHeight;
        var newSize = newWidth * newHeight;
        
        _drawBuffer = new CellData[newSize];
        _previousBuffer = new CellData[newSize];
        
        // Initialize with empty cells
        for (var i = 0; i < newSize; i++)
        {
            _drawBuffer[i] = CellData.Empty;
            _previousBuffer[i] = CellData.Empty;
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
        var positionsToUpdate = new List<(Position pos, CellData data)>();

        // Check all positions for changes
        for (var y = 0; y < _bufferHeight; y++)
        {
            for (var x = 0; x < _bufferWidth; x++)
            {
                var index = y * _bufferWidth + x;
                var newData = _drawBuffer[index];
                var oldData = _previousBuffer[index];
                
                if (newData != oldData)
                {
                    var pos = new Position(x, y);
                    positionsToUpdate.Add((pos, newData));
                }
            }
        }

        // Sort by position for optimal cursor movement
        positionsToUpdate.Sort((a, b) =>
        {
            var yCompare = a.pos.Y.CompareTo(b.pos.Y);
            return yCompare != 0 ? yCompare : a.pos.X.CompareTo(b.pos.X);
        });

        // Update only changed positions
        foreach (var (pos, cellData) in positionsToUpdate)
        {
            // Handle empty cells - clear them completely
            if (cellData.IsEmpty)
            {
                System.Console.SetCursorPosition(pos.X, pos.Y);
                
                if (ConsoleSurface.SupportsColor)
                {
                    if (_supports24BitColor)
                    {
                        // Reset to default colors and write space
                        System.Console.Write(AnsiReset + " ");
                    }
                    else
                    {
                        System.Console.ForegroundColor = ConsoleColor.Gray;
                        System.Console.BackgroundColor = ConsoleColor.Black;
                        System.Console.Write(' ');
                    }
                }
                else
                {
                    System.Console.Write(' ');
                }
                continue;
            }
            
            var character = cellData.Character;
            var fg = cellData.Foreground;
            var bg = cellData.Background;
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
        Array.Copy(_drawBuffer, _previousBuffer, _drawBuffer.Length);
    }

    private void RenderToRedirectedOutput()
    {
        // For redirected output, render a complete frame line by line
        for (var y = 0; y < _surface.Height; y++)
        {
            _frameBuffer.Clear();

            for (var x = 0; x < _surface.Width; x++)
            {
                var index = y * _bufferWidth + x;
                var cellData = _drawBuffer[index];
                
                if (!cellData.IsEmpty)
                {
                    _frameBuffer.Append(cellData.Character);
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
