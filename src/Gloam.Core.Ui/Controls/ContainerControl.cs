using Gloam.Core.Primitives;

namespace Gloam.Core.Ui.Controls;

/// <summary>
///     A container control that holds and manages child controls
/// </summary>
public class ContainerControl : BaseControl
{
    /// <summary>
    ///     Initializes a new instance of ContainerControl
    /// </summary>
    /// <param name="position">The position of the container</param>
    /// <param name="size">The size of the container</param>
    public ContainerControl(Position position, Size size) : base(position, size)
    {
        Background = Colors.Transparent;
        Foreground = Colors.White;
    }

    /// <summary>
    ///     Initializes a new instance of ContainerControl at origin with default size
    /// </summary>
    public ContainerControl() : this(new Position(0, 0), new Size(100, 100))
    {
    }

    /// <inheritdoc />
    protected override void RenderContent(IGuiRenderer renderer)
    {
        // Container control only renders its background (if not transparent)
        // Children are rendered by the base class
        
        // Optionally render a border if the container has a visible foreground
        if (Foreground.A > 0 && Background.A > 0)
        {
            var borderColor = IsFocused ? Colors.Yellow : Foreground;
            renderer.DrawRectangle(Position, Size, borderColor);
        }
    }

    /// <summary>
    ///     Arranges child controls within the container using automatic layout
    /// </summary>
    /// <param name="layoutType">The type of layout to apply</param>
    public void ArrangeChildren(ContainerLayoutType layoutType = ContainerLayoutType.None)
    {
        if (!Children.Any())
            return;

        switch (layoutType)
        {
            case ContainerLayoutType.Vertical:
                ArrangeChildrenVertically();
                break;
            case ContainerLayoutType.Horizontal:
                ArrangeChildrenHorizontally();
                break;
            case ContainerLayoutType.Grid:
                ArrangeChildrenInGrid();
                break;
            case ContainerLayoutType.None:
            default:
                // No automatic layout
                break;
        }

        Invalidate();
    }

    private void ArrangeChildrenVertically()
    {
        var currentY = 0;
        var padding = 2;

        foreach (var child in Children)
        {
            child.Position = new Position(padding, currentY + padding);
            child.Size = new Size(Size.Width - (padding * 2), child.Size.Height);
            currentY += child.Size.Height + padding;
        }
    }

    private void ArrangeChildrenHorizontally()
    {
        var currentX = 0;
        var padding = 2;

        foreach (var child in Children)
        {
            child.Position = new Position(currentX + padding, padding);
            child.Size = new Size(child.Size.Width, Size.Height - (padding * 2));
            currentX += child.Size.Width + padding;
        }
    }

    private void ArrangeChildrenInGrid()
    {
        var columns = (int)Math.Ceiling(Math.Sqrt(Children.Count));
        var rows = (int)Math.Ceiling((double)Children.Count / columns);
        var cellWidth = Size.Width / columns;
        var cellHeight = Size.Height / rows;
        var padding = 2;

        for (var i = 0; i < Children.Count; i++)
        {
            var row = i / columns;
            var col = i % columns;
            
            var child = Children[i];
            child.Position = new Position(
                col * cellWidth + padding,
                row * cellHeight + padding
            );
            child.Size = new Size(
                cellWidth - (padding * 2),
                cellHeight - (padding * 2)
            );
        }
    }

    /// <summary>
    ///     Finds the child control at the specified position
    /// </summary>
    /// <param name="position">The position to test</param>
    /// <returns>The child control at the position, or null if none found</returns>
    public IGuiControl? GetChildAt(Position position)
    {
        // Test children in reverse draw order (topmost first)
        var sortedChildren = Children
            .Where(c => c.IsVisible)
            .OrderByDescending(c => c.DrawOrder)
            .ToList();

        foreach (var child in sortedChildren)
        {
            if (child.Contains(position))
            {
                return child;
            }
        }

        return null;
    }
}

/// <summary>
///     Defines the layout types for container controls
/// </summary>
public enum ContainerLayoutType
{
    /// <summary>No automatic layout</summary>
    None,
    /// <summary>Arrange children vertically</summary>
    Vertical,
    /// <summary>Arrange children horizontally</summary>
    Horizontal,
    /// <summary>Arrange children in a grid</summary>
    Grid
}