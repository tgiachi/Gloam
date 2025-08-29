using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Core.Ui;

namespace Gloam.Console.Render.Rendering;

/// <summary>
///     Console implementation of IGuiRenderer for rendering GUI controls to console
/// </summary>
public class ConsoleGuiRenderer : IGuiRenderer
{
    private IRenderer? _renderer;
    private Rectangle? _clipRegion;

    /// <summary>
    ///     Initializes a new instance of ConsoleGuiRenderer
    /// </summary>
    public ConsoleGuiRenderer()
    {
    }

    /// <summary>
    ///     Sets the renderer to use for drawing (called from the render context)
    /// </summary>
    /// <param name="renderer">The renderer to use</param>
    public void SetRenderer(IRenderer renderer)
    {
        _renderer = renderer;
    }

    /// <inheritdoc />
    public void FillRectangle(Position position, Size size, Color color)
    {
        if (_renderer == null || color.A == 0) // No renderer or transparent, don't render
            return;

        var startX = Math.Max(0, position.X);
        var startY = Math.Max(0, position.Y);
        var endX = Math.Min(_renderer.Surface.Width, position.X + size.Width);
        var endY = Math.Min(_renderer.Surface.Height, position.Y + size.Height);

        // Apply clipping if active
        if (_clipRegion.HasValue)
        {
            var clip = _clipRegion.Value;
            startX = Math.Max(startX, clip.X);
            startY = Math.Max(startY, clip.Y);
            endX = Math.Min(endX, clip.X + clip.Width);
            endY = Math.Min(endY, clip.Y + clip.Height);
        }

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                _renderer.DrawText(new Position(x, y), " ", Colors.White, color);
            }
        }
    }

    /// <inheritdoc />
    public void DrawRectangle(Position position, Size size, Color color)
    {
        if (_renderer == null || color.A == 0 || size.Width < 1 || size.Height < 1)
            return;

        var right = position.X + size.Width - 1;
        var bottom = position.Y + size.Height - 1;

        // Draw horizontal lines
        for (var x = position.X; x <= right; x++)
        {
            if (IsInClipRegion(x, position.Y))
                _renderer.DrawText(new Position(x, position.Y), "─", color, Colors.Transparent);
            if (IsInClipRegion(x, bottom) && bottom != position.Y)
                _renderer.DrawText(new Position(x, bottom), "─", color, Colors.Transparent);
        }

        // Draw vertical lines
        for (var y = position.Y; y <= bottom; y++)
        {
            if (IsInClipRegion(position.X, y))
                _renderer.DrawText(new Position(position.X, y), "│", color, Colors.Transparent);
            if (IsInClipRegion(right, y) && right != position.X)
                _renderer.DrawText(new Position(right, y), "│", color, Colors.Transparent);
        }

        // Draw corners
        if (IsInClipRegion(position.X, position.Y))
            _renderer.DrawText(new Position(position.X, position.Y), "┌", color, Colors.Transparent);
        if (IsInClipRegion(right, position.Y))
            _renderer.DrawText(new Position(right, position.Y), "┐", color, Colors.Transparent);
        if (IsInClipRegion(position.X, bottom))
            _renderer.DrawText(new Position(position.X, bottom), "└", color, Colors.Transparent);
        if (IsInClipRegion(right, bottom))
            _renderer.DrawText(new Position(right, bottom), "┘", color, Colors.Transparent);
    }

    /// <inheritdoc />
    public void DrawText(string text, Position position, Color color)
    {
        if (_renderer == null || string.IsNullOrEmpty(text) || color.A == 0)
            return;

        var x = position.X;
        var y = position.Y;

        foreach (var ch in text)
        {
            if (IsInClipRegion(x, y))
            {
                _renderer.DrawText(new Position(x, y), ch.ToString(), color, Colors.Transparent);
            }
            x++;

            // Stop if we're outside the surface or clip region
            if (x >= _renderer.Surface.Width || (_clipRegion.HasValue && x >= _clipRegion.Value.X + _clipRegion.Value.Width))
                break;
        }
    }

    /// <inheritdoc />
    public void SetClipRegion(Rectangle rectangle)
    {
        if (_renderer == null) return;
        
        // Clamp clip region to surface bounds
        var clampedX = Math.Max(0, rectangle.X);
        var clampedY = Math.Max(0, rectangle.Y);
        var clampedWidth = Math.Min(rectangle.Width, _renderer.Surface.Width - clampedX);
        var clampedHeight = Math.Min(rectangle.Height, _renderer.Surface.Height - clampedY);

        if (clampedWidth > 0 && clampedHeight > 0)
        {
            _clipRegion = new Rectangle(clampedX, clampedY, clampedWidth, clampedHeight);
        }
        else
        {
            _clipRegion = null;
        }
    }

    /// <inheritdoc />
    public void ClearClipRegion()
    {
        _clipRegion = null;
    }

    private bool IsInClipRegion(int x, int y)
    {
        if (_renderer == null) return false;
        
        if (!_clipRegion.HasValue)
            return x >= 0 && y >= 0 && x < _renderer.Surface.Width && y < _renderer.Surface.Height;

        var clip = _clipRegion.Value;
        return x >= clip.X && y >= clip.Y && 
               x < clip.X + clip.Width && y < clip.Y + clip.Height;
    }
}