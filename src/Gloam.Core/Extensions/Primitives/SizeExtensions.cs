using Gloam.Core.Primitives;

namespace Gloam.Core.Extensions.Primitives;

/// <summary>
///     Extension methods for Size operations
/// </summary>
public static class SizeExtensions
{
    /// <summary>
    ///     Checks if a position is within the bounds of a size (assuming origin at 0,0)
    /// </summary>
    public static bool Contains(this Size size, Position position)
    {
        return position.X >= 0 && position.X < size.Width &&
               position.Y >= 0 && position.Y < size.Height;
    }

    /// <summary>
    ///     Checks if a position is within the bounds of a size with a specific origin
    /// </summary>
    public static bool Contains(this Size size, Position position, Position origin)
    {
        return position.X >= origin.X && position.X < origin.X + size.Width &&
               position.Y >= origin.Y && position.Y < origin.Y + size.Height;
    }

    /// <summary>
    ///     Creates a rectangle from a size with the specified origin
    /// </summary>
    public static Rectangle ToRectangle(this Size size, Position origin = default)
    {
        return new Rectangle(origin.X, origin.Y, size.Width, size.Height);
    }

    /// <summary>
    ///     Gets all positions within the size bounds (for iteration)
    /// </summary>
    public static IEnumerable<Position> GetAllPositions(this Size size)
    {
        for (var y = 0; y < size.Height; y++)
        {
            for (var x = 0; x < size.Width; x++)
            {
                yield return new Position(x, y);
            }
        }
    }

    /// <summary>
    ///     Gets the center position of the size
    /// </summary>
    public static Position GetCenter(this Size size)
    {
        return new Position(size.Width / 2, size.Height / 2);
    }
}
