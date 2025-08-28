using Gloam.Core.Interfaces;
using Gloam.Core.Types;

namespace Gloam.Console.Render.Surfaces;

/// <summary>
/// Console-based render surface that adapts to the current console window size
/// </summary>
public sealed class ConsoleSurface : IRenderSurface
{
    private int _width;
    private int _height;

    /// <summary>
    /// Initializes a new console surface with current console dimensions
    /// </summary>
    public ConsoleSurface()
    {
        UpdateDimensions();
        
        // Monitor console resize events if supported
        try
        {
            System.Console.CancelKeyPress += (_, _) => { };  // Enable console events
        }
        catch
        {
            // Ignore if console events are not supported
        }
    }

    /// <inheritdoc />
    public int Width => _width;

    /// <inheritdoc />
    public int Height => _height;

    /// <inheritdoc />
    public RenderUnit Unit => RenderUnit.Cells;

    /// <inheritdoc />
    public int CellWidth => 1;

    /// <inheritdoc />
    public int CellHeight => 1;

    /// <inheritdoc />
    public event Action<int, int>? Resized;

    /// <summary>
    /// Updates the surface dimensions from the current console window size
    /// </summary>
    public void UpdateDimensions()
    {
        var previousWidth = _width;
        var previousHeight = _height;

        try
        {
            _width = System.Console.WindowWidth;
            _height = System.Console.WindowHeight;
        }
        catch
        {
            // Fallback to default console dimensions if not available
            _width = 80;
            _height = 25;
        }

        if (previousWidth != _width || previousHeight != _height)
        {
            Resized?.Invoke(_width, _height);
        }
    }

    /// <summary>
    /// Gets whether the console supports color output
    /// </summary>
    public static bool SupportsColor => !System.Console.IsOutputRedirected;

    /// <summary>
    /// Gets whether the console supports cursor positioning
    /// </summary>
    public static bool SupportsCursorPositioning => !System.Console.IsOutputRedirected;
}