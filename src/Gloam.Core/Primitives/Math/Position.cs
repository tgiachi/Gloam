using System.Numerics;

namespace Gloam.Core.Primitives.Math;

/// <summary>
/// Represents a position in 2D space with both discrete grid coordinates and continuous offsets for smooth movement.
/// </summary>
public readonly struct Position : IEquatable<Position>
{
    public int X { get; }
    public int Y { get; }
    public float OffsetX { get; }
    public float OffsetY { get; }

    public Position(int x, int y, float offsetX = 0f, float offsetY = 0f)
    {
        X = x;
        Y = y;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public Position(Vector2 vector)
    {
        X = (int)MathF.Floor(vector.X);
        Y = (int)MathF.Floor(vector.Y);
        OffsetX = vector.X - X;
        OffsetY = vector.Y - Y;
    }

    public float TotalX => X + OffsetX;
    public float TotalY => Y + OffsetY;

    public Vector2 AsVector2() => new(TotalX, TotalY);

    public Position WithOffset(float offsetX, float offsetY) => 
        new(X, Y, offsetX, offsetY);

    public Position WithGridPosition(int x, int y) => 
        new(x, y, OffsetX, OffsetY);

    public Position Move(int deltaX, int deltaY) => 
        new(X + deltaX, Y + deltaY, OffsetX, OffsetY);

    public Position Move(float deltaX, float deltaY)
    {
        var newTotalX = TotalX + deltaX;
        var newTotalY = TotalY + deltaY;
        return new Position(new Vector2(newTotalX, newTotalY));
    }

    public Position Move(Vector2 delta) => Move(delta.X, delta.Y);

    public static Position operator +(Position position, Vector2 vector) => 
        position.Move(vector);

    public static Position operator -(Position position, Vector2 vector) => 
        position.Move(-vector);

    public static Vector2 operator -(Position a, Position b) => 
        a.AsVector2() - b.AsVector2();

    public static implicit operator Vector2(Position position) => 
        position.AsVector2();

    public static implicit operator Position(Vector2 vector) => 
        new(vector);

    public bool Equals(Position other) =>
        X == other.X && Y == other.Y && 
        OffsetX.Equals(other.OffsetX) && OffsetY.Equals(other.OffsetY);

    public override bool Equals(object? obj) => 
        obj is Position other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(X, Y, OffsetX, OffsetY);

    public static bool operator ==(Position left, Position right) => 
        left.Equals(right);

    public static bool operator !=(Position left, Position right) => 
        !left.Equals(right);

    public override string ToString() => 
        $"Position({X}, {Y}, {OffsetX:F2}, {OffsetY:F2})";
}