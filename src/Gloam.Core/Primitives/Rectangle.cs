using System.Runtime.CompilerServices;

namespace Gloam.Core.Primitives;

/// <summary>
/// Represents a rectangle with integer coordinates optimized for roguelike grid-based operations.
/// Follows KISS principles with minimal overhead and high performance for frequent allocations.
/// </summary>
public readonly struct Rectangle : IEquatable<Rectangle>
{
    #region Fields

    /// <summary>
    /// The X coordinate of the rectangle's top-left corner.
    /// </summary>
    public readonly int X;

    /// <summary>
    /// The Y coordinate of the rectangle's top-left corner.
    /// </summary>
    public readonly int Y;

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public readonly int Height;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the X coordinate of the left edge of the rectangle.
    /// </summary>
    public int Left => X;

    /// <summary>
    /// Gets the Y coordinate of the top edge of the rectangle.
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// Gets the X coordinate of the right edge of the rectangle.
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    /// Gets the Y coordinate of the bottom edge of the rectangle.
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    /// Gets the center point of the rectangle.
    /// </summary>
    public Position Center => new(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Gets the area of the rectangle.
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Gets whether the rectangle is empty (has zero or negative area).
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Gets an empty rectangle at origin.
    /// </summary>
    public static Rectangle Empty => new(0, 0, 0, 0);

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new rectangle with the specified position and size.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner.</param>
    /// <param name="y">The Y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new rectangle with the specified position and size.
    /// </summary>
    /// <param name="position">The top-left corner position.</param>
    /// <param name="size">The size of the rectangle.</param>
    public Rectangle(Position position, Size size) : this(position.X, position.Y, size.Width, size.Height)
    {
    }

    /// <summary>
    /// Initializes a new rectangle from two corner points.
    /// </summary>
    /// <param name="topLeft">The top-left corner.</param>
    /// <param name="bottomRight">The bottom-right corner.</param>
    public static Rectangle FromCorners(Position topLeft, Position bottomRight)
    {
        return new Rectangle(
            topLeft.X,
            topLeft.Y,
            bottomRight.X - topLeft.X,
            bottomRight.Y - topLeft.Y
        );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the specified point is contained within this rectangle.
    /// </summary>
    /// <param name="x">The X coordinate of the point.</param>
    /// <param name="y">The Y coordinate of the point.</param>
    /// <returns>True if the point is contained within the rectangle; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int x, int y)
    {
        return x >= X && x < Right && y >= Y && y < Bottom;
    }

    /// <summary>
    /// Determines whether the specified point is contained within this rectangle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the point is contained within the rectangle; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Position point)
    {
        return Contains(point.X, point.Y);
    }

    /// <summary>
    /// Determines whether the specified rectangle is entirely contained within this rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangle is entirely contained; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Rectangle other)
    {
        return other.X >= X && other.Y >= Y &&
               other.Right <= Right && other.Bottom <= Bottom;
    }

    /// <summary>
    /// Determines whether this rectangle intersects with another rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test for intersection.</param>
    /// <returns>True if the rectangles intersect; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(Rectangle other)
    {
        return X < other.Right && Right > other.X &&
               Y < other.Bottom && Bottom > other.Y;
    }

    /// <summary>
    /// Returns the intersection of this rectangle with another rectangle.
    /// </summary>
    /// <param name="other">The rectangle to intersect with.</param>
    /// <returns>The intersection rectangle, or Empty if no intersection exists.</returns>
    public Rectangle Intersect(Rectangle other)
    {
        int x = Math.Max(X, other.X);
        int y = Math.Max(Y, other.Y);
        int right = Math.Min(Right, other.Right);
        int bottom = Math.Min(Bottom, other.Bottom);

        if (right <= x || bottom <= y)
            return Empty;

        return new Rectangle(x, y, right - x, bottom - y);
    }

    /// <summary>
    /// Returns the union of this rectangle with another rectangle.
    /// </summary>
    /// <param name="other">The rectangle to union with.</param>
    /// <returns>The union rectangle.</returns>
    public Rectangle Union(Rectangle other)
    {
        int x = Math.Min(X, other.X);
        int y = Math.Min(Y, other.Y);
        int right = Math.Max(Right, other.Right);
        int bottom = Math.Max(Bottom, other.Bottom);

        return new Rectangle(x, y, right - x, bottom - y);
    }

    /// <summary>
    /// Returns a new rectangle with the specified offset applied.
    /// </summary>
    /// <param name="offsetX">The X offset.</param>
    /// <param name="offsetY">The Y offset.</param>
    /// <returns>The offset rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rectangle Offset(int offsetX, int offsetY)
    {
        return new Rectangle(X + offsetX, Y + offsetY, Width, Height);
    }

    /// <summary>
    /// Returns a new rectangle with the specified offset applied.
    /// </summary>
    /// <param name="offset">The offset point.</param>
    /// <returns>The offset rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rectangle Offset(Position offset)
    {
        return Offset(offset.X, offset.Y);
    }

    /// <summary>
    /// Returns a new rectangle inflated by the specified amount.
    /// </summary>
    /// <param name="width">The amount to inflate horizontally.</param>
    /// <param name="height">The amount to inflate vertically.</param>
    /// <returns>The inflated rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rectangle Inflate(int width, int height)
    {
        return new Rectangle(X - width, Y - height, Width + width * 2, Height + height * 2);
    }

    #endregion

    #region Equality

    /// <summary>
    /// Indicates whether this rectangle is equal to another rectangle.
    /// </summary>
    /// <param name="other">The rectangle to compare with.</param>
    /// <returns>True if the rectangles are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Rectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    /// <summary>
    /// Indicates whether this rectangle is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Rectangle other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this rectangle.
    /// </summary>
    /// <returns>A hash code for this rectangle.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    /// <summary>
    /// Returns a string representation of this rectangle.
    /// </summary>
    /// <returns>A string representation of this rectangle.</returns>
    public override string ToString()
    {
        return $"Rectangle({X}, {Y}, {Width}, {Height})";
    }

    #endregion

    #region Operators

    /// <summary>
    /// Determines whether two rectangles are equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns>True if the rectangles are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two rectangles are not equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns>True if the rectangles are not equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }

    #endregion
}
