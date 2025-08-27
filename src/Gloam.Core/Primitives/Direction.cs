using System.Numerics;

namespace Gloam.Core.Primitives;

/// <summary>
/// Represents a direction in 2D space using 8-directional movement (cardinal and diagonal directions).
/// </summary>
public readonly struct Direction : IEquatable<Direction>
{
    public static readonly Direction North = new(0, -1);
    public static readonly Direction NorthEast = new(1, -1);
    public static readonly Direction East = new(1, 0);
    public static readonly Direction SouthEast = new(1, 1);
    public static readonly Direction South = new(0, 1);
    public static readonly Direction SouthWest = new(-1, 1);
    public static readonly Direction West = new(-1, 0);
    public static readonly Direction NorthWest = new(-1, -1);

    public static readonly Direction[] All8 =
    [
        North, NorthEast, East, SouthEast,
        South, SouthWest, West, NorthWest
    ];

    public int X { get; }
    public int Y { get; }

    public Direction(int x, int y)
    {
        X = x == 0 ? 0 : x > 0 ? 1 : -1;
        Y = y == 0 ? 0 : y > 0 ? 1 : -1;
    }

    public Vector2 AsVector2() => new(X, Y);

    public static Position operator +(Position position, Direction direction) =>
        position.Move(direction.X, direction.Y);

    public static implicit operator Vector2(Direction direction) =>
        direction.AsVector2();

    public bool Equals(Direction other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Direction other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Direction left, Direction right) => left.Equals(right);
    public static bool operator !=(Direction left, Direction right) => !left.Equals(right);

    public override string ToString() => this switch
    {
        var d when d == North => "North",
        var d when d == NorthEast => "NorthEast",
        var d when d == East => "East",
        var d when d == SouthEast => "SouthEast",
        var d when d == South => "South",
        var d when d == SouthWest => "SouthWest",
        var d when d == West => "West",
        var d when d == NorthWest => "NorthWest",
        _ => $"Direction({X}, {Y})"
    };
}
