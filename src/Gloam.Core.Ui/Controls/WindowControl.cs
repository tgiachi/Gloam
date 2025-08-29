using Gloam.Core.Primitives;

namespace Gloam.Core.Ui.Controls;

/// <summary>
///     A window control with ASCII borders and optional title bar
/// </summary>
public class WindowControl : BaseControl
{
    private string _title = string.Empty;
    private bool _showTitle = true;

    /// <summary>
    ///     Initializes a new instance of WindowControl
    /// </summary>
    /// <param name="position">The position of the window</param>
    /// <param name="size">The size of the window</param>
    /// <param name="title">The window title</param>
    public WindowControl(Position position, Size size, string title = "") : base(position, size)
    {
        _title = title ?? string.Empty;
        Background = Colors.Black;
        Foreground = Colors.White;
        BorderColor = Colors.Gray;
        TitleColor = Colors.Yellow;
    }

    /// <summary>
    ///     Initializes a new instance of WindowControl with default size
    /// </summary>
    /// <param name="position">The position of the window</param>
    /// <param name="title">The window title</param>
    public WindowControl(Position position, string title = "") : this(position, new Size(40, 20), title)
    {
    }

    /// <summary>
    ///     Gets or sets the window title
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            var newTitle = value ?? string.Empty;
            if (_title != newTitle)
            {
                _title = newTitle;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets whether to show the title bar
    /// </summary>
    public bool ShowTitle
    {
        get => _showTitle;
        set
        {
            if (_showTitle != value)
            {
                _showTitle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the border color
    /// </summary>
    public Color BorderColor { get; set; }

    /// <summary>
    ///     Gets or sets the title text color
    /// </summary>
    public Color TitleColor { get; set; }

    /// <summary>
    ///     Gets the content area inside the window borders
    /// </summary>
    public Rectangle ContentArea
    {
        get
        {
            var titleOffset = ShowTitle && !string.IsNullOrEmpty(Title) ? 1 : 0;
            return new Rectangle(
                Position.X + 1,
                Position.Y + 1 + titleOffset,
                Math.Max(0, Size.Width - 2),
                Math.Max(0, Size.Height - 2 - titleOffset)
            );
        }
    }

    /// <inheritdoc />
    protected override void RenderContent(IGuiRenderer renderer)
    {
        if (Size.Width < 3 || Size.Height < 3)
            return;

        // Render window background
        if (Background.A > 0)
        {
            var contentFill = new Rectangle(Position.X + 1, Position.Y + 1, Size.Width - 2, Size.Height - 2);
            renderer.FillRectangle(new Position(contentFill.X, contentFill.Y), new Size(contentFill.Width, contentFill.Height), Background);
        }

        // Render borders
        RenderBorders(renderer);

        // Render title if enabled
        if (ShowTitle && !string.IsNullOrEmpty(Title))
        {
            RenderTitle(renderer);
        }
    }

    private void RenderBorders(IGuiRenderer renderer)
    {
        var left = Position.X;
        var top = Position.Y;
        var right = Position.X + Size.Width - 1;
        var bottom = Position.Y + Size.Height - 1;

        // Draw corners
        renderer.DrawText("┌", new Position(left, top), BorderColor);
        renderer.DrawText("┐", new Position(right, top), BorderColor);
        renderer.DrawText("└", new Position(left, bottom), BorderColor);
        renderer.DrawText("┘", new Position(right, bottom), BorderColor);

        // Draw horizontal borders
        for (var x = left + 1; x < right; x++)
        {
            renderer.DrawText("─", new Position(x, top), BorderColor);
            renderer.DrawText("─", new Position(x, bottom), BorderColor);
        }

        // Draw vertical borders
        for (var y = top + 1; y < bottom; y++)
        {
            renderer.DrawText("│", new Position(left, y), BorderColor);
            renderer.DrawText("│", new Position(right, y), BorderColor);
        }

        // If showing title, draw separator line
        if (ShowTitle && !string.IsNullOrEmpty(Title) && Size.Height > 3)
        {
            var titleSeparatorY = top + 2;
            if (titleSeparatorY < bottom)
            {
                renderer.DrawText("├", new Position(left, titleSeparatorY), BorderColor);
                for (var x = left + 1; x < right; x++)
                {
                    renderer.DrawText("─", new Position(x, titleSeparatorY), BorderColor);
                }
                renderer.DrawText("┤", new Position(right, titleSeparatorY), BorderColor);
            }
        }
    }

    private void RenderTitle(IGuiRenderer renderer)
    {
        if (Size.Width <= 4) // Not enough space for title
            return;

        var titleY = Position.Y + 1;
        var availableWidth = Size.Width - 4; // Account for borders and padding
        var displayTitle = Title.Length > availableWidth ? Title[..availableWidth] : Title;

        // Center the title
        var titleX = Position.X + 2 + Math.Max(0, (availableWidth - displayTitle.Length) / 2);
        renderer.DrawText(displayTitle, new Position(titleX, titleY), TitleColor);
    }

    /// <summary>
    ///     Adds a child control positioned within the window's content area
    /// </summary>
    /// <param name="child">The child control to add</param>
    public override void AddChild(IGuiControl child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!Children.Contains(child))
        {
            // Adjust child position to be relative to content area
            var contentArea = ContentArea;
            var adjustedPosition = new Position(
                contentArea.X + child.Position.X,
                contentArea.Y + child.Position.Y,
                child.Position.OffsetX,
                child.Position.OffsetY
            );
            child.Position = adjustedPosition;

            Children.Add(child);
            child.Parent = this;
            Invalidate();
        }
    }

    /// <summary>
    ///     Sets the window title and optionally resizes to fit
    /// </summary>
    /// <param name="title">The new title</param>
    /// <param name="autoResize">Whether to resize the window to fit the title</param>
    public void SetTitle(string title, bool autoResize = false)
    {
        Title = title;

        if (autoResize && !string.IsNullOrEmpty(title))
        {
            var minWidth = title.Length + 6; // Title + borders + padding
            if (Size.Width < minWidth)
            {
                Size = new Size(minWidth, Size.Height);
            }
        }
    }

    /// <summary>
    ///     Gets whether a point is within the content area of the window
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <returns>True if the point is within the content area</returns>
    public bool ContainsInContentArea(Position point)
    {
        return ContentArea.Contains(point);
    }

    /// <summary>
    ///     Gets whether a point is within the title bar area
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <returns>True if the point is within the title bar</returns>
    public bool ContainsInTitleBar(Position point)
    {
        if (!ShowTitle || string.IsNullOrEmpty(Title))
            return false;

        var titleArea = new Rectangle(Position.X + 1, Position.Y + 1, Size.Width - 2, 1);
        return titleArea.Contains(point);
    }

    /// <summary>
    ///     Sets the window style with predefined colors
    /// </summary>
    /// <param name="style">The window style to apply</param>
    public void SetStyle(WindowStyle style)
    {
        switch (style)
        {
            case WindowStyle.Default:
                Background = Colors.Black;
                Foreground = Colors.White;
                BorderColor = Colors.Gray;
                TitleColor = Colors.Yellow;
                break;
                
            case WindowStyle.Dialog:
                Background = Colors.DarkBlue;
                Foreground = Colors.White;
                BorderColor = Colors.Cyan;
                TitleColor = Colors.White;
                break;
                
            case WindowStyle.Error:
                Background = Colors.DarkRed;
                Foreground = Colors.White;
                BorderColor = Colors.Red;
                TitleColor = Colors.Yellow;
                break;
                
            case WindowStyle.Success:
                Background = Colors.DarkGreen;
                Foreground = Colors.White;
                BorderColor = Colors.Green;
                TitleColor = Colors.White;
                break;
        }
        
        Invalidate();
    }
}

/// <summary>
///     Predefined window styles
/// </summary>
public enum WindowStyle
{
    /// <summary>Default window style</summary>
    Default,
    /// <summary>Dialog window style</summary>
    Dialog,
    /// <summary>Error window style</summary>
    Error,
    /// <summary>Success window style</summary>
    Success
}