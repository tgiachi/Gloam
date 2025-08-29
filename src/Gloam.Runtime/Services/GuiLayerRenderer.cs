using Gloam.Console.Render.Rendering;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Ui;
using Gloam.Core.Ui.Controls;

namespace Gloam.Runtime.Services;

/// <summary>
///     A layer renderer that manages and renders GUI controls
/// </summary>
public class GuiLayerRenderer : BaseLayerRenderer, IGuiLayerRenderer
{
    private readonly List<IGuiControl> _controls = new();
    private readonly IGuiRenderer _guiRenderer;

    /// <summary>
    ///     Initializes a new instance of GuiLayerRenderer
    /// </summary>
    /// <param name="guiRenderer">The GUI renderer to use for drawing controls</param>
    /// <param name="priority">The layer priority (default: 1000)</param>
    /// <param name="name">The name of the layer (default: "GUI")</param>
    public GuiLayerRenderer(IGuiRenderer guiRenderer, int priority = 1000, string name = "GUI")
    {
        _guiRenderer = guiRenderer ?? throw new ArgumentNullException(nameof(guiRenderer));
        Priority = priority;
        Name = name;
    }

    /// <inheritdoc />
    public override int Priority { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public int ControlCount => _controls.Count;

    /// <inheritdoc />
    public void AddControl(object control)
    {
        if (control is IGuiControl guiControl)
        {
            if (!_controls.Contains(guiControl))
            {
                _controls.Add(guiControl);
                SortControlsByDrawOrder();
            }
        }
        else
        {
            throw new ArgumentException("Control must implement IGuiControl", nameof(control));
        }
    }

    /// <inheritdoc />
    public bool RemoveControl(object control)
    {
        if (control is IGuiControl guiControl)
        {
            return _controls.Remove(guiControl);
        }
        return false;
    }

    /// <inheritdoc />
    public void ClearControls()
    {
        _controls.Clear();
    }

    /// <inheritdoc />
    public bool ContainsControl(object control)
    {
        if (control is IGuiControl guiControl)
        {
            return _controls.Contains(guiControl);
        }
        return false;
    }

    /// <inheritdoc />
    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        if (_controls.Count == 0)
            return;

        ct.ThrowIfCancellationRequested();

        // Handle input first
        HandleInput(context);

        // Set the renderer for the GUI renderer if it supports it
        if (_guiRenderer is ConsoleGuiRenderer consoleGuiRenderer)
        {
            consoleGuiRenderer.SetRenderer(context.Renderer);
        }

        // Update all controls
        var deltaTime = context.FrameInfo.DeltaTime;
        foreach (var control in _controls.Where(c => c.IsVisible))
        {
            ct.ThrowIfCancellationRequested();
            control.Update(context.InputDevice, deltaTime);
        }

        // Render all controls in draw order
        foreach (var control in _controls.Where(c => c.IsVisible))
        {
            ct.ThrowIfCancellationRequested();
            control.Render(_guiRenderer);
        }

        await ValueTask.CompletedTask;
    }

    private void SortControlsByDrawOrder()
    {
        _controls.Sort((a, b) => a.DrawOrder.CompareTo(b.DrawOrder));
    }

    /// <summary>
    ///     Finds the topmost control at the specified position
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns>The topmost control at the position, or null if none found</returns>
    public IGuiControl? GetControlAt(Gloam.Core.Primitives.Position position)
    {
        // Check controls in reverse draw order (topmost first)
        for (var i = _controls.Count - 1; i >= 0; i--)
        {
            var control = _controls[i];
            if (control.IsVisible && control.Contains(position))
            {
                return control;
            }
        }

        return null;
    }

    /// <summary>
    ///     Sets focus to the specified control, removing focus from all others
    /// </summary>
    /// <param name="control">The control to focus, or null to clear focus</param>
    public void SetFocus(IGuiControl? control)
    {
        // Clear all focus first (including nested controls)
        foreach (var c in _controls)
        {
            ClearFocusRecursive(c);
        }
        
        // Set focus on the target control
        if (control != null)
        {
            control.IsFocused = true;
        }
    }
    
    /// <summary>
    ///     Recursively clears focus from a control and all its children
    /// </summary>
    /// <param name="control">The control to clear focus from</param>
    private static void ClearFocusRecursive(IGuiControl control)
    {
        control.IsFocused = false;
        foreach (var child in control.Children)
        {
            ClearFocusRecursive(child);
        }
    }

    /// <summary>
    ///     Gets the currently focused control
    /// </summary>
    /// <returns>The focused control, or null if no control has focus</returns>
    public IGuiControl? GetFocusedControl()
    {
        return _controls.FirstOrDefault(c => c.IsFocused);
    }

    /// <summary>
    ///     Handles input for GUI controls, including focus management and input routing
    /// </summary>
    /// <param name="context">The rendering context containing input information</param>
    public void HandleInput(RenderLayerContext context)
    {
        var inputDevice = context.InputDevice;
        
        
        // Handle mouse input for focus management
        var mouseState = inputDevice.Mouse;
        if (mouseState.Pressed && mouseState.Button == Gloam.Core.Types.MouseButtonType.Left)
        {
            var mousePosition = new Gloam.Core.Primitives.Position(mouseState.X, mouseState.Y);
            
            var clickedControl = GetControlAt(mousePosition);
            SetFocus(clickedControl);
        }

        // Tab key navigation
        if (inputDevice.WasPressed(Gloam.Core.Input.Keys.Tab))
        {
            var isShift = inputDevice.IsDown(Gloam.Core.Input.Keys.LeftShift) || inputDevice.IsDown(Gloam.Core.Input.Keys.RightShift);
            NavigateFocus(isShift);
        }
    }

    private void NavigateFocus(bool reverse)
    {
        // Get all focusable controls in the hierarchy (depth-first)
        var focusableControls = new List<IGuiControl>();
        foreach (var rootControl in _controls.Where(c => c.IsVisible))
        {
            CollectFocusableControls(rootControl, focusableControls);
        }

        if (focusableControls.Count == 0)
            return;

        var currentIndex = focusableControls.FindIndex(c => c.IsFocused);
        
        int nextIndex;
        if (currentIndex == -1)
        {
            // No current focus, select first or last
            nextIndex = reverse ? focusableControls.Count - 1 : 0;
        }
        else
        {
            // Move focus (cycle around)
            nextIndex = reverse 
                ? (currentIndex == 0 ? focusableControls.Count - 1 : currentIndex - 1)
                : (currentIndex == focusableControls.Count - 1 ? 0 : currentIndex + 1);
        }

        var targetControl = focusableControls[nextIndex];
        SetFocus(targetControl);
        
        // Debug: Verify the focus was actually set
        #if DEBUG
        System.Diagnostics.Debug.WriteLine($"TAB Focus set to: {targetControl.GetType().Name} at ({targetControl.Position.X}, {targetControl.Position.Y}), IsFocused: {targetControl.IsFocused}");
        #endif
    }

    /// <summary>
    ///     Recursively collects all focusable controls from the control hierarchy
    /// </summary>
    /// <param name="control">The control to examine</param>
    /// <param name="focusableControls">The list to add focusable controls to</param>
    private static void CollectFocusableControls(IGuiControl control, List<IGuiControl> focusableControls)
    {
        if (!control.IsVisible)
            return;

        // Add the control if it's focusable (for now, consider all visible controls focusable)
        // In a real implementation, you might want to add an IsFocusable property to IGuiControl
        if (IsFocusableControl(control))
        {
            focusableControls.Add(control);
        }

        // Recursively add children
        foreach (var child in control.Children)
        {
            CollectFocusableControls(child, focusableControls);
        }
    }

    /// <summary>
    ///     Determines if a control can receive focus
    /// </summary>
    /// <param name="control">The control to check</param>
    /// <returns>True if the control can be focused</returns>
    private static bool IsFocusableControl(IGuiControl control)
    {
        // Only interactive controls should be focusable
        // Container controls (WindowControl, ContainerControl) should NOT be focusable
        // because they are just layout containers - focus should go to their children
        return control is EditBox || 
               control is ProgressBar;
        // Note: TextBox is not focusable as it's read-only text display
    }
}