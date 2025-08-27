using Gloam.Core.Types;

namespace Gloam.Core.Interfaces;

/// <summary>
/// Interface representing a render surface that can be drawn to, supporting both cell-based and pixel-based rendering
/// </summary>
public interface IRenderSurface
{
    /// <summary>
    /// Gets the width of the render surface in the current unit (cells or pixels)
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the render surface in the current unit (cells or pixels)
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the render unit type (Cells for console, Pixels for GPU)
    /// </summary>
    RenderUnit Unit { get; }

    /// <summary>
    /// Gets the width of a single cell (1 for console-based rendering)
    /// </summary>
    int CellWidth { get; }

    /// <summary>
    /// Gets the height of a single cell (1 for console-based rendering)
    /// </summary>
    int CellHeight { get; }

    /// <summary>
    /// Event triggered when the render surface is resized
    /// </summary>
    event Action<int, int>? Resized;
}
