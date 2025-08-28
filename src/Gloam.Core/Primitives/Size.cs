using System.Numerics;

namespace Gloam.Core.Primitives;

/// <summary>
///     Represents a size in 2D space with width and height dimensions
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    /// <summary>
    ///     Gets the width component
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///     Gets the height component
    /// </summary>
    public int Height { get; }

    /// <summary>
    ///     Initializes a new Size with the specified width and height
    /// </summary>
    /// <param name="width">The width dimension (must be >= 0)</param>
    /// <param name="height">The height dimension (must be >= 0)</param>
    public Size(int width, int height)
    {
        Width = Math.Max(0, width); // Clamp to prevent negative sizes
        Height = Math.Max(0, height);
    }

    /// <summary>
    ///     Gets the total area (width * height)
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    ///     Gets whether this size has zero area
    /// </summary>
    public bool IsEmpty => Width == 0 || Height == 0;

    /// <summary>
    ///     Gets whether this size represents a square (width == height)
    /// </summary>
    public bool IsSquare => Width == Height && Width > 0;

    #region Static Properties

    /// <summary>
    ///     Gets a Size with zero width and height
    /// </summary>
    public static Size Empty => new(0, 0);

    /// <summary>
    ///     Gets a Size representing a single unit (1x1)
    /// </summary>
    public static Size Unit => new(1, 1);

    #endregion

    #region Conversion Methods

    /// <summary>
    ///     Converts the size to a Vector2 with width as X and height as Y
    /// </summary>
    public Vector2 ToVector2()
    {
        return new Vector2(Width, Height);
    }

    /// <summary>
    ///     Creates a Size from a Vector2, taking the floor of fractional components
    /// </summary>
    public static Size FromVector2(Vector2 vector)
    {
        return new Size(
            (int)MathF.Floor(Math.Max(0, vector.X)),
            (int)MathF.Floor(Math.Max(0, vector.Y))
        );
    }

    /// <summary>
    ///     Converts to a Position with Width as X and Height as Y
    /// </summary>
    public Position ToPosition()
    {
        return new Position(Width, Height);
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    ///     Adds two sizes component-wise
    /// </summary>
    public static Size operator +(Size left, Size right)
    {
        return new Size(left.Width + right.Width, left.Height + right.Height);
    }

    /// <summary>
    ///     Subtracts two sizes component-wise
    /// </summary>
    public static Size operator -(Size left, Size right)
    {
        return new Size(left.Width - right.Width, left.Height - right.Height);
    }

    /// <summary>
    ///     Scales a size by a factor
    /// </summary>
    public static Size operator *(Size size, int scale)
    {
        return new Size(size.Width * scale, size.Height * scale);
    }

    /// <summary>
    ///     Scales a size by a factor
    /// </summary>
    public static Size operator *(int scale, Size size)
    {
        return size * scale;
    }

    /// <summary>
    ///     Scales a size by a fractional factor
    /// </summary>
    public static Size operator *(Size size, float scale)
    {
        return new Size(
            (int)MathF.Round(size.Width * scale),
            (int)MathF.Round(size.Height * scale)
        );
    }

    /// <summary>
    ///     Divides a size by a divisor
    /// </summary>
    public static Size operator /(Size size, int divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide size by zero");
        }

        return new Size(size.Width / divisor, size.Height / divisor);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    ///     Creates a new Size with the specified width, keeping the same height
    /// </summary>
    public Size WithWidth(int newWidth)
    {
        return new Size(newWidth, Height);
    }

    /// <summary>
    ///     Creates a new Size with the specified height, keeping the same width
    /// </summary>
    public Size WithHeight(int newHeight)
    {
        return new Size(Width, newHeight);
    }

    /// <summary>
    ///     Creates a Size that fits within the specified bounds while maintaining aspect ratio
    /// </summary>
    public Size FitWithin(Size bounds)
    {
        if (IsEmpty || bounds.IsEmpty)
        {
            return Empty;
        }

        var scaleX = (float)bounds.Width / Width;
        var scaleY = (float)bounds.Height / Height;
        var scale = Math.Min(scaleX, scaleY);

        return new Size(
            (int)MathF.Round(Width * scale),
            (int)MathF.Round(Height * scale)
        );
    }

    /// <summary>
    ///     Creates a Size that fills the specified bounds while maintaining aspect ratio (may exceed bounds)
    /// </summary>
    public Size FillBounds(Size bounds)
    {
        if (IsEmpty || bounds.IsEmpty)
        {
            return Empty;
        }

        var scaleX = (float)bounds.Width / Width;
        var scaleY = (float)bounds.Height / Height;
        var scale = Math.Max(scaleX, scaleY);

        return new Size(
            (int)MathF.Round(Width * scale),
            (int)MathF.Round(Height * scale)
        );
    }

    /// <summary>
    ///     Expands the size to include the specified size
    /// </summary>
    public Size Union(Size other)
    {
        return new Size(
            Math.Max(Width, other.Width),
            Math.Max(Height, other.Height)
        );
    }

    /// <summary>
    ///     Creates a Size that represents the intersection of two sizes (minimum of each dimension)
    /// </summary>
    public Size Intersect(Size other)
    {
        return new Size(
            Math.Min(Width, other.Width),
            Math.Min(Height, other.Height)
        );
    }

    /// <summary>
    ///     Clamps the size to be within the specified minimum and maximum bounds
    /// </summary>
    public Size Clamp(Size minimum, Size maximum)
    {
        return new Size(
            Math.Clamp(Width, minimum.Width, maximum.Width),
            Math.Clamp(Height, minimum.Height, maximum.Height)
        );
    }

    #endregion

    #region Implicit Conversions

    /// <summary>
    ///     Implicit conversion to Vector2
    /// </summary>
    public static implicit operator Vector2(Size size)
    {
        return size.ToVector2();
    }

    /// <summary>
    ///     Explicit conversion from Vector2
    /// </summary>
    public static explicit operator Size(Vector2 vector)
    {
        return FromVector2(vector);
    }

    #endregion

    #region Equality and Comparison

    public bool Equals(Size other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is Size other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public static bool operator ==(Size left, Size right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Size left, Size right)
    {
        return !left.Equals(right);
    }

    #endregion

    #region String Representation

    public override string ToString()
    {
        return $"Size({Width}, {Height})";
    }

    /// <summary>
    ///     Returns a string representation with area information
    /// </summary>
    public string ToDetailedString()
    {
        return $"Size({Width}x{Height}, Area: {Area})";
    }

    #endregion
}
