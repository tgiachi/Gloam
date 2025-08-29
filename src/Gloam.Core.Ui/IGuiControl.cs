using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;

namespace Gloam.Core.Ui;

/// <summary>
///     Represents a GUI control that can be rendered, updated, and interacted with
/// </summary>
public interface IGuiControl
{
    /// <summary>
    ///     Gets or sets the position of the control
    /// </summary>
    Position Position { get; set; }

    /// <summary>
    ///     Gets or sets the size of the control
    /// </summary>
    Size Size { get; set; }

    /// <summary>
    ///     Gets or sets whether the control currently has focus
    /// </summary>
    bool IsFocused { get; set; }

    /// <summary>
    ///     Gets or sets the drawing order priority (higher values drawn later)
    /// </summary>
    int DrawOrder { get; set; }

    /// <summary>
    ///     Gets or sets the background color of the control
    /// </summary>
    Color Background { get; set; }

    /// <summary>
    ///     Gets or sets the foreground color of the control
    /// </summary>
    Color Foreground { get; set; }

    /// <summary>
    ///     Gets or sets whether the control needs to be redrawn
    /// </summary>
    bool IsDirty { get; set; }

    /// <summary>
    ///     Gets or sets whether the control is visible
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    ///     Gets the collection of child controls
    /// </summary>
    IList<IGuiControl> Children { get; }

    /// <summary>
    ///     Gets or sets the parent control
    /// </summary>
    IGuiControl? Parent { get; set; }

    /// <summary>
    ///     Renders the control and its children
    /// </summary>
    /// <param name="renderer">The renderer to use for drawing</param>
    void Render(IGuiRenderer renderer);

    /// <summary>
    ///     Updates the control state based on input and game logic
    /// </summary>
    /// <param name="inputDevice">The input device for handling user input</param>
    /// <param name="deltaTime">The time elapsed since the last update</param>
    void Update(IInputDevice inputDevice, TimeSpan deltaTime);

    /// <summary>
    ///     Adds a child control to this control
    /// </summary>
    /// <param name="child">The child control to add</param>
    void AddChild(IGuiControl child);

    /// <summary>
    ///     Removes a child control from this control
    /// </summary>
    /// <param name="child">The child control to remove</param>
    /// <returns>True if the child was removed, false if it was not found</returns>
    bool RemoveChild(IGuiControl child);

    /// <summary>
    ///     Marks the control as dirty, requiring a redraw
    /// </summary>
    void Invalidate();

    /// <summary>
    ///     Determines if a point is within the bounds of this control
    /// </summary>
    /// <param name="point">The point to test</param>
    /// <returns>True if the point is within the control bounds</returns>
    bool Contains(Position point);
}