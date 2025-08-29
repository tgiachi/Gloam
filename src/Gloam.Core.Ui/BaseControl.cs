using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;

namespace Gloam.Core.Ui;

/// <summary>
///     Abstract base implementation of IGuiControl providing common functionality
/// </summary>
public abstract class BaseControl : IGuiControl
{
    private Position _position;
    private Size _size;
    private bool _isFocused;
    private bool _isDirty = true;
    private bool _isVisible = true;
    private Color _background;
    private Color _foreground;
    private IGuiControl? _parent;

    /// <summary>
    ///     Initializes a new instance of BaseControl
    /// </summary>
    /// <param name="position">The initial position</param>
    /// <param name="size">The initial size</param>
    protected BaseControl(Position position, Size size)
    {
        _position = position;
        _size = size;
        _background = Colors.Transparent;
        _foreground = Colors.White;
        Children = new List<IGuiControl>();
        DrawOrder = 0;
    }

    /// <inheritdoc />
    public Position Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public Size Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public bool IsFocused
    {
        get => _isFocused;
        set
        {
            if (_isFocused != value)
            {
                _isFocused = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public int DrawOrder { get; set; }

    /// <inheritdoc />
    public Color Background
    {
        get => _background;
        set
        {
            if (_background.GetHashCode() != value.GetHashCode())
            {
                _background = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public Color Foreground
    {
        get => _foreground;
        set
        {
            if (_foreground.GetHashCode() != value.GetHashCode())
            {
                _foreground = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public bool IsDirty
    {
        get => _isDirty;
        set => _isDirty = value;
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public IList<IGuiControl> Children { get; }

    /// <inheritdoc />
    public IGuiControl? Parent
    {
        get => _parent;
        set
        {
            if (_parent != value)
            {
                _parent = value;
                Invalidate();
            }
        }
    }

    /// <inheritdoc />
    public virtual void Render(IGuiRenderer renderer)
    {
        if (!IsVisible)
            return;

        // Render background if not transparent
        if (Background.A > 0)
        {
            renderer.FillRectangle(Position, Size, Background);
        }

        // Render the control content
        RenderContent(renderer);

        // Render children in draw order
        var sortedChildren = Children
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DrawOrder)
            .ToList();

        foreach (var child in sortedChildren)
        {
            child.Render(renderer);
        }

        IsDirty = false;
    }

    /// <summary>
    ///     Renders the specific content of this control (override in derived classes)
    /// </summary>
    /// <param name="renderer">The renderer to use for drawing</param>
    protected abstract void RenderContent(IGuiRenderer renderer);

    /// <inheritdoc />
    public virtual void Update(IInputDevice inputDevice, TimeSpan deltaTime)
    {
        if (!IsVisible)
            return;

        // Update this control
        UpdateContent(inputDevice, deltaTime);

        // Update children
        foreach (var child in Children)
        {
            child.Update(inputDevice, deltaTime);
        }
    }

    /// <summary>
    ///     Updates the specific content of this control (override in derived classes)
    /// </summary>
    /// <param name="inputDevice">The input device for handling user input</param>
    /// <param name="deltaTime">The time elapsed since the last update</param>
    protected virtual void UpdateContent(IInputDevice inputDevice, TimeSpan deltaTime)
    {
        // Default implementation does nothing
    }

    /// <inheritdoc />
    public virtual void AddChild(IGuiControl child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!Children.Contains(child))
        {
            Children.Add(child);
            child.Parent = this;
            Invalidate();
        }
    }

    /// <inheritdoc />
    public virtual bool RemoveChild(IGuiControl child)
    {
        if (child == null)
            return false;

        if (Children.Remove(child))
        {
            child.Parent = null;
            Invalidate();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public virtual void Invalidate()
    {
        IsDirty = true;
        
        // Propagate invalidation up the parent chain
        Parent?.Invalidate();
    }

    /// <inheritdoc />
    public virtual bool Contains(Position point)
    {
        var bounds = new Rectangle(Position, Size);
        return bounds.Contains(point);
    }

    /// <summary>
    ///     Gets the absolute position of this control (including parent offsets)
    /// </summary>
    /// <returns>The absolute position</returns>
    protected Position GetAbsolutePosition()
    {
        var absolutePosition = Position;
        var parent = Parent;

        while (parent != null)
        {
            absolutePosition = new Position(
                absolutePosition.X + parent.Position.X,
                absolutePosition.Y + parent.Position.Y,
                absolutePosition.OffsetX + parent.Position.OffsetX,
                absolutePosition.OffsetY + parent.Position.OffsetY
            );
            parent = parent.Parent;
        }

        return absolutePosition;
    }
}