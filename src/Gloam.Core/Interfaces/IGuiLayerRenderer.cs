using Gloam.Core.Contexts;

namespace Gloam.Core.Interfaces;

/// <summary>
///     Interface for rendering GUI layers that contain GUI controls
/// </summary>
public interface IGuiLayerRenderer : ILayerRenderer
{
    /// <summary>
    ///     Adds a GUI control to this layer
    /// </summary>
    /// <param name="control">The control to add</param>
    void AddControl(object control);

    /// <summary>
    ///     Removes a GUI control from this layer
    /// </summary>
    /// <param name="control">The control to remove</param>
    /// <returns>True if the control was removed, false if it was not found</returns>
    bool RemoveControl(object control);

    /// <summary>
    ///     Clears all GUI controls from this layer
    /// </summary>
    void ClearControls();

    /// <summary>
    ///     Gets the number of controls in this layer
    /// </summary>
    int ControlCount { get; }

    /// <summary>
    ///     Gets whether this layer contains the specified control
    /// </summary>
    /// <param name="control">The control to check for</param>
    /// <returns>True if the control is in this layer</returns>
    bool ContainsControl(object control);
}