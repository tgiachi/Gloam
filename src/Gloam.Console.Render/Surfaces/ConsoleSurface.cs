using Gloam.Core.Interfaces;
using Gloam.Core.Types;

namespace Gloam.Console.Render.Surfaces;

/// <summary>
///     Console-based render surface that adapts to the current console window size
/// </summary>
public sealed class ConsoleSurface : IRenderSurface
{
    /// <summary>
    ///     Initializes a new console surface with current console dimensions
    /// </summary>
    public ConsoleSurface()
    {
        UpdateDimensions();

        // Monitor console resize events if supported
        try
        {
            System.Console.CancelKeyPress += (_, _) => { }; // Enable console events
        }
        catch
        {
            // Ignore if console events are not supported
        }
    }

    /// <summary>
    ///     Gets whether the console supports color output
    /// </summary>
    public static bool SupportsColor => !System.Console.IsOutputRedirected;

    /// <summary>
    ///     Gets whether the console supports cursor positioning
    /// </summary>
    public static bool SupportsCursorPositioning => !System.Console.IsOutputRedirected;

    /// <inheritdoc />
    public int Width { get; private set; }

    /// <inheritdoc />
    public int Height { get; private set; }

    /// <inheritdoc />
    public RenderUnit Unit => RenderUnit.Cells;

    /// <inheritdoc />
    public int CellWidth => 1;

    /// <inheritdoc />
    public int CellHeight => 1;

    /// <inheritdoc />
    public event Action<int, int>? Resized;

    /// <summary>
    ///     Updates the surface dimensions from the current console window size
    /// </summary>
    public void UpdateDimensions()
    {
        var previousWidth = Width;
        var previousHeight = Height;

        try
        {
            Width = System.Console.WindowWidth;
            Height = System.Console.WindowHeight;
        }
        catch
        {
            // Fallback to default console dimensions if not available
            Width = 80;
            Height = 25;
        }

        if (previousWidth != Width || previousHeight != Height)
        {
            Resized?.Invoke(Width, Height);
        }
    }
}
