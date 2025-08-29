using Gloam.Core.Primitives;

namespace Gloam.Core.Ui.Controls;

/// <summary>
///     A read-only text display control that supports multi-line text and text wrapping
/// </summary>
public class TextBox : BaseControl
{
    private string _text = string.Empty;
    private string[] _lines = Array.Empty<string>();
    private int _scrollOffset;
    private bool _wordWrap = true;

    /// <summary>
    ///     Initializes a new instance of TextBox
    /// </summary>
    /// <param name="position">The position of the text box</param>
    /// <param name="size">The size of the text box</param>
    public TextBox(Position position, Size size) : base(position, size)
    {
        Background = Colors.Transparent;
        Foreground = Colors.White;
    }

    /// <summary>
    ///     Initializes a new instance of TextBox with default size
    /// </summary>
    /// <param name="position">The position of the text box</param>
    public TextBox(Position position) : this(position, new Size(40, 10))
    {
    }

    /// <summary>
    ///     Gets or sets the text content of the text box
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            var newText = value ?? string.Empty;
            if (_text != newText)
            {
                _text = newText;
                UpdateLines();
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets whether text should wrap at word boundaries
    /// </summary>
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap != value)
            {
                _wordWrap = value;
                UpdateLines();
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the vertical scroll offset in lines
    /// </summary>
    public int ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            var maxScroll = Math.Max(0, _lines.Length - Size.Height + 2);
            var newOffset = Math.Clamp(value, 0, maxScroll);
            if (_scrollOffset != newOffset)
            {
                _scrollOffset = newOffset;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets the total number of lines in the text
    /// </summary>
    public int LineCount => _lines.Length;

    /// <summary>
    ///     Gets whether the text box can scroll vertically
    /// </summary>
    public bool CanScrollVertically => _lines.Length > Size.Height - 2;

    /// <summary>
    ///     Gets or sets the text alignment
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    /// <inheritdoc />
    protected override void RenderContent(IGuiRenderer renderer)
    {
        if (string.IsNullOrEmpty(_text))
            return;

        // Calculate content area (accounting for borders if background is visible)
        var contentPosition = Background.A > 0 
            ? new Position(Position.X + 1, Position.Y + 1)
            : Position;
        
        var contentWidth = Background.A > 0 ? Size.Width - 2 : Size.Width;
        var contentHeight = Background.A > 0 ? Size.Height - 2 : Size.Height;

        // Render border if background is visible
        if (Background.A > 0)
        {
            renderer.DrawRectangle(Position, Size, Foreground);
        }

        // Set clipping region
        var clipRect = new Rectangle(contentPosition, new Size(contentWidth, contentHeight));
        renderer.SetClipRegion(clipRect);

        try
        {
            // Render visible lines
            var endLine = Math.Min(_lines.Length, _scrollOffset + contentHeight);
            
            for (var i = _scrollOffset; i < endLine; i++)
            {
                var line = _lines[i];
                if (string.IsNullOrEmpty(line))
                    continue;

                var linePosition = new Position(
                    contentPosition.X + GetHorizontalOffset(line, contentWidth),
                    contentPosition.Y + (i - _scrollOffset)
                );

                renderer.DrawText(line, linePosition, Foreground);
            }
        }
        finally
        {
            renderer.ClearClipRegion();
        }
    }

    private int GetHorizontalOffset(string line, int contentWidth)
    {
        return Alignment switch
        {
            TextAlignment.Center => Math.Max(0, (contentWidth - line.Length) / 2),
            TextAlignment.Right => Math.Max(0, contentWidth - line.Length),
            _ => 0 // Left alignment
        };
    }

    private void UpdateLines()
    {
        if (string.IsNullOrEmpty(_text))
        {
            _lines = Array.Empty<string>();
            return;
        }

        var contentWidth = Background.A > 0 ? Size.Width - 2 : Size.Width;
        var originalLines = _text.Split('\n', StringSplitOptions.None);
        var processedLines = new List<string>();

        foreach (var originalLine in originalLines)
        {
            if (string.IsNullOrEmpty(originalLine))
            {
                processedLines.Add(string.Empty);
                continue;
            }

            if (!WordWrap || originalLine.Length <= contentWidth)
            {
                processedLines.Add(originalLine);
                continue;
            }

            // Wrap the line
            var wrappedLines = WrapLine(originalLine, contentWidth);
            processedLines.AddRange(wrappedLines);
        }

        _lines = processedLines.ToArray();
        
        // Adjust scroll offset if necessary
        var maxScroll = Math.Max(0, _lines.Length - Size.Height + 2);
        if (_scrollOffset > maxScroll)
        {
            _scrollOffset = maxScroll;
        }
    }

    private static List<string> WrapLine(string line, int maxWidth)
    {
        var lines = new List<string>();
        var words = line.Split(' ');
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            
            if (testLine.Length <= maxWidth)
            {
                currentLine = testLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    // Single word longer than max width - break it
                    var remainingWord = word;
                    while (remainingWord.Length > maxWidth)
                    {
                        lines.Add(remainingWord[..maxWidth]);
                        remainingWord = remainingWord[maxWidth..];
                    }
                    currentLine = remainingWord;
                }
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    /// <summary>
    ///     Scrolls the text up by the specified number of lines
    /// </summary>
    /// <param name="lines">Number of lines to scroll up</param>
    public void ScrollUp(int lines = 1)
    {
        ScrollOffset = Math.Max(0, ScrollOffset - lines);
    }

    /// <summary>
    ///     Scrolls the text down by the specified number of lines
    /// </summary>
    /// <param name="lines">Number of lines to scroll down</param>
    public void ScrollDown(int lines = 1)
    {
        ScrollOffset += lines;
    }

    /// <summary>
    ///     Scrolls to the top of the text
    /// </summary>
    public void ScrollToTop()
    {
        ScrollOffset = 0;
    }

    /// <summary>
    ///     Scrolls to the bottom of the text
    /// </summary>
    public void ScrollToBottom()
    {
        var maxScroll = Math.Max(0, _lines.Length - Size.Height + 2);
        ScrollOffset = maxScroll;
    }

    /// <summary>
    ///     Appends text to the current content
    /// </summary>
    /// <param name="text">The text to append</param>
    public void AppendText(string text)
    {
        Text += text ?? string.Empty;
    }

    /// <summary>
    ///     Appends a line of text to the current content
    /// </summary>
    /// <param name="line">The line to append</param>
    public void AppendLine(string line = "")
    {
        Text += (string.IsNullOrEmpty(_text) ? "" : "\n") + (line ?? string.Empty);
    }
}

/// <summary>
///     Defines text alignment options
/// </summary>
public enum TextAlignment
{
    /// <summary>Left-aligned text</summary>
    Left,
    /// <summary>Center-aligned text</summary>
    Center,
    /// <summary>Right-aligned text</summary>
    Right
}