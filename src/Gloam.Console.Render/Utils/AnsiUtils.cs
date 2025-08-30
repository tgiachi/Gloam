using System.Text;
using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Utils;

/// <summary>
///     Utility class for generating ANSI escape sequences using Gloam's Position and Color types
/// </summary>
public static class AnsiUtils
{
    private const string ESC = "\x1b";
    
    /// <summary>
    ///     ANSI Control Sequence Introducer
    /// </summary>
    private const string CSI = ESC + "[";

    #region Screen Control
    
    /// <summary>
    ///     Clear entire screen
    /// </summary>
    public static string ClearScreen => CSI + "2J";
    
    /// <summary>
    ///     Clear current line
    /// </summary>
    public static string ClearLine => CSI + "2K";
    
    /// <summary>
    ///     Move cursor to home position (1,1)
    /// </summary>
    public static string Home => CSI + "H";
    
    /// <summary>
    ///     Enable alternate screen buffer
    /// </summary>
    public static string EnableAltScreen => CSI + "?1049h";
    
    /// <summary>
    ///     Disable alternate screen buffer
    /// </summary>
    public static string DisableAltScreen => CSI + "?1049l";
    
    #endregion

    #region Cursor Control
    
    /// <summary>
    ///     Hide cursor
    /// </summary>
    public static string HideCursor => CSI + "?25l";
    
    /// <summary>
    ///     Show cursor
    /// </summary>
    public static string ShowCursor => CSI + "?25h";
    
    /// <summary>
    ///     Move cursor to specific position (1-based coordinates)
    /// </summary>
    /// <param name="position">Position to move to (X,Y grid coordinates)</param>
    public static string MoveCursor(Position position)
    {
        return MoveCursor(position.X + 1, position.Y + 1);
    }
    
    /// <summary>
    ///     Move cursor to specific position (1-based coordinates)
    /// </summary>
    /// <param name="x">Column (1-based)</param>
    /// <param name="y">Row (1-based)</param>
    public static string MoveCursor(int x, int y)
    {
        return $"{CSI}{y};{x}H";
    }
    
    #endregion

    #region Colors
    
    /// <summary>
    ///     Set foreground color using 24-bit RGB
    /// </summary>
    /// <param name="color">Color to set</param>
    public static string SetForegroundColor(Color color)
    {
        return $"{CSI}38;2;{color.R};{color.G};{color.B}m";
    }
    
    /// <summary>
    ///     Set background color using 24-bit RGB
    /// </summary>
    /// <param name="color">Color to set</param>
    public static string SetBackgroundColor(Color color)
    {
        return $"{CSI}48;2;{color.R};{color.G};{color.B}m";
    }
    
    /// <summary>
    ///     Reset all colors and formatting to default
    /// </summary>
    public static string Reset => CSI + "0m";
    
    /// <summary>
    ///     Reset foreground color to default
    /// </summary>
    public static string ResetForeground => CSI + "39m";
    
    /// <summary>
    ///     Reset background color to default
    /// </summary>
    public static string ResetBackground => CSI + "49m";
    
    #endregion

    #region Text Formatting
    
    /// <summary>
    ///     Make text bold
    /// </summary>
    public static string Bold => CSI + "1m";
    
    /// <summary>
    ///     Make text dim
    /// </summary>
    public static string Dim => CSI + "2m";
    
    /// <summary>
    ///     Make text italic
    /// </summary>
    public static string Italic => CSI + "3m";
    
    /// <summary>
    ///     Underline text
    /// </summary>
    public static string Underline => CSI + "4m";
    
    /// <summary>
    ///     Reset bold
    /// </summary>
    public static string ResetBold => CSI + "22m";
    
    /// <summary>
    ///     Reset all text formatting
    /// </summary>
    public static string ResetFormatting => Reset;
    
    #endregion

    #region Mouse Support
    
    /// <summary>
    ///     Enable mouse tracking
    /// </summary>
    public static string EnableMouseTracking => CSI + "?1003h";
    
    /// <summary>
    ///     Disable mouse tracking
    /// </summary>
    public static string DisableMouseTracking => CSI + "?1003l";
    
    /// <summary>
    ///     Enable SGR mouse mode
    /// </summary>
    public static string EnableSGRMouse => CSI + "?1006h";
    
    /// <summary>
    ///     Disable SGR mouse mode
    /// </summary>
    public static string DisableSGRMouse => CSI + "?1006l";
    
    /// <summary>
    ///     Enable focus events
    /// </summary>
    public static string EnableFocusEvents => CSI + "?1004h";
    
    /// <summary>
    ///     Disable focus events
    /// </summary>
    public static string DisableFocusEvents => CSI + "?1004l";
    
    #endregion

    #region Terminal Configuration
    
    /// <summary>
    ///     Disable line wrapping
    /// </summary>
    public static string DisableLineWrap => CSI + "?7l";
    
    /// <summary>
    ///     Enable line wrapping
    /// </summary>
    public static string EnableLineWrap => CSI + "?7h";
    
    #endregion

    #region High-Level Operations
    
    /// <summary>
    ///     Initialize terminal for roguelike game (alternate screen, hide cursor, etc.)
    /// </summary>
    public static string InitializeTerminal()
    {
        var sb = new StringBuilder();
        sb.Append(EnableAltScreen);
        sb.Append(ClearScreen);
        sb.Append(Home);
        sb.Append(HideCursor);
        sb.Append(DisableLineWrap);
        sb.Append(EnableMouseTracking);
        sb.Append(EnableSGRMouse);
        sb.Append(EnableFocusEvents);
        return sb.ToString();
    }
    
    /// <summary>
    ///     Restore terminal to normal state
    /// </summary>
    public static string RestoreTerminal()
    {
        var sb = new StringBuilder();
        sb.Append(ShowCursor);
        sb.Append(EnableLineWrap);
        sb.Append(DisableMouseTracking);
        sb.Append(DisableSGRMouse);
        sb.Append(DisableFocusEvents);
        sb.Append(Reset);
        sb.Append(DisableAltScreen);
        return sb.ToString();
    }
    
    /// <summary>
    ///     Write colored text at specific position
    /// </summary>
    /// <param name="position">Position to write at</param>
    /// <param name="text">Text to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color (optional)</param>
    public static string WriteAt(Position position, string text, Color foreground, Color? background = null)
    {
        var sb = new StringBuilder();
        sb.Append(MoveCursor(position));
        sb.Append(SetForegroundColor(foreground));
        if (background.HasValue)
        {
            sb.Append(SetBackgroundColor(background.Value));
        }
        sb.Append(text);
        sb.Append(Reset);
        return sb.ToString();
    }
    
    /// <summary>
    ///     Write a single character at specific position with colors
    /// </summary>
    /// <param name="position">Position to write at</param>
    /// <param name="character">Character to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color (optional)</param>
    public static string WriteCharAt(Position position, char character, Color foreground, Color? background = null)
    {
        return WriteAt(position, character.ToString(), foreground, background);
    }
    
    #endregion
}