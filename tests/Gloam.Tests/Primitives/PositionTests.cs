using System.Numerics;
using Gloam.Core.Primitives;

namespace Gloam.Tests.Primitives;

/// <summary>
/// Tests for the Position struct.
/// </summary>
public class PositionTests
{
    [Test]
    public void Constructor_WithIntsOnly_ShouldInitializeCorrectly()
    {
        var position = new Position(5, -3);

        Assert.That(position.X, Is.EqualTo(5));
        Assert.That(position.Y, Is.EqualTo(-3));
        Assert.That(position.OffsetX, Is.EqualTo(0f));
        Assert.That(position.OffsetY, Is.EqualTo(0f));
    }

    [Test]
    public void Constructor_WithIntsAndOffsets_ShouldInitializeCorrectly()
    {
        var position = new Position(2, 7, 0.25f, -0.75f);

        Assert.That(position.X, Is.EqualTo(2));
        Assert.That(position.Y, Is.EqualTo(7));
        Assert.That(position.OffsetX, Is.EqualTo(0.25f));
        Assert.That(position.OffsetY, Is.EqualTo(-0.75f));
    }

    [Test]
    public void Constructor_WithVector2_ShouldSplitIntoGridAndOffset()
    {
        var vector = new Vector2(3.75f, -2.25f);
        var position = new Position(vector);

        Assert.That(position.X, Is.EqualTo(3));
        Assert.That(position.Y, Is.EqualTo(-3)); // Floor of -2.25
        Assert.That(position.OffsetX, Is.EqualTo(0.75f));
        Assert.That(position.OffsetY, Is.EqualTo(0.75f)); // -2.25 - (-3) = 0.75
    }

    [Test]
    public void Constructor_WithNegativeVector2_ShouldHandleCorrectly()
    {
        var vector = new Vector2(-1.3f, -0.7f);
        var position = new Position(vector);

        Assert.That(position.X, Is.EqualTo(-2)); // Floor of -1.3
        Assert.That(position.Y, Is.EqualTo(-1)); // Floor of -0.7
        Assert.That(position.OffsetX, Is.EqualTo(0.7f).Within(0.001f));
        Assert.That(position.OffsetY, Is.EqualTo(0.3f).Within(0.001f));
    }

    [Test]
    public void TotalX_ShouldReturnXPlusOffsetX()
    {
        var position = new Position(5, 3, 0.25f, 0.75f);

        Assert.That(position.TotalX, Is.EqualTo(5.25f));
    }

    [Test]
    public void TotalY_ShouldReturnYPlusOffsetY()
    {
        var position = new Position(5, 3, 0.25f, 0.75f);

        Assert.That(position.TotalY, Is.EqualTo(3.75f));
    }

    [Test]
    public void AsVector2_ShouldReturnTotalCoordinates()
    {
        var position = new Position(2, -1, 0.5f, 0.25f);
        var vector = position.AsVector2();

        Assert.That(vector.X, Is.EqualTo(2.5f));
        Assert.That(vector.Y, Is.EqualTo(-0.75f));
    }

    [Test]
    public void WithOffset_ShouldUpdateOffsetsOnly()
    {
        var original = new Position(3, 4, 0.1f, 0.2f);
        var updated = original.WithOffset(0.8f, 0.9f);

        Assert.That(updated.X, Is.EqualTo(3));
        Assert.That(updated.Y, Is.EqualTo(4));
        Assert.That(updated.OffsetX, Is.EqualTo(0.8f));
        Assert.That(updated.OffsetY, Is.EqualTo(0.9f));
    }

    [Test]
    public void WithGridPosition_ShouldUpdateGridCoordinatesOnly()
    {
        var original = new Position(1, 2, 0.3f, 0.4f);
        var updated = original.WithGridPosition(5, 6);

        Assert.That(updated.X, Is.EqualTo(5));
        Assert.That(updated.Y, Is.EqualTo(6));
        Assert.That(updated.OffsetX, Is.EqualTo(0.3f));
        Assert.That(updated.OffsetY, Is.EqualTo(0.4f));
    }

    [Test]
    public void Move_WithInts_ShouldMoveGridPosition()
    {
        var position = new Position(3, 2, 0.5f, 0.25f);
        var moved = position.Move(2, -1);

        Assert.That(moved.X, Is.EqualTo(5));
        Assert.That(moved.Y, Is.EqualTo(1));
        Assert.That(moved.OffsetX, Is.EqualTo(0.5f));
        Assert.That(moved.OffsetY, Is.EqualTo(0.25f));
    }

    [Test]
    public void Move_WithFloats_ShouldRecalculateGridAndOffset()
    {
        var position = new Position(2, 3, 0.25f, 0.75f);
        var moved = position.Move(0.8f, -0.5f);

        // Original total: (2.25, 3.75), after move: (3.05, 3.25)
        Assert.That(moved.X, Is.EqualTo(3)); // Floor of 3.05
        Assert.That(moved.Y, Is.EqualTo(3)); // Floor of 3.25
        Assert.That(moved.OffsetX, Is.EqualTo(0.05f).Within(0.001f));
        Assert.That(moved.OffsetY, Is.EqualTo(0.25f).Within(0.001f));
    }

    [Test]
    public void Move_WithVector2_ShouldCallFloatVersion()
    {
        var position = new Position(1, 1, 0.5f, 0.5f);
        var delta = new Vector2(0.7f, -0.3f);
        var moved = position.Move(delta);

        // Original total: (1.5, 1.5), after move: (2.2, 1.2)
        Assert.That(moved.X, Is.EqualTo(2)); // Floor of 2.2
        Assert.That(moved.Y, Is.EqualTo(1)); // Floor of 1.2
        Assert.That(moved.OffsetX, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(moved.OffsetY, Is.EqualTo(0.2f).Within(0.001f));
    }

    [Test]
    public void AddOperator_WithVector2_ShouldMovePosition()
    {
        var position = new Position(2, 3);
        var delta = new Vector2(1.5f, -0.5f);
        var result = position + delta;

        Assert.That(result.X, Is.EqualTo(3)); // Floor of 3.5
        Assert.That(result.Y, Is.EqualTo(2)); // Floor of 2.5
        Assert.That(result.OffsetX, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(result.OffsetY, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void SubtractOperator_WithVector2_ShouldMovePositionNegatively()
    {
        var position = new Position(5, 4, 0.25f, 0.75f);
        var delta = new Vector2(1.5f, 2.0f);
        var result = position - delta;

        // Original total: (5.25, 4.75), after subtract: (3.75, 2.75)
        Assert.That(result.X, Is.EqualTo(3)); // Floor of 3.75
        Assert.That(result.Y, Is.EqualTo(2)); // Floor of 2.75
        Assert.That(result.OffsetX, Is.EqualTo(0.75f).Within(0.001f));
        Assert.That(result.OffsetY, Is.EqualTo(0.75f).Within(0.001f));
    }

    [Test]
    public void SubtractOperator_TwoPositions_ShouldReturnVector2Difference()
    {
        var pos1 = new Position(5, 3, 0.25f, 0.75f); // Total: (5.25, 3.75)
        var pos2 = new Position(2, 1, 0.5f, 0.25f);  // Total: (2.5, 1.25)
        var difference = pos1 - pos2;

        Assert.That(difference.X, Is.EqualTo(2.75f));
        Assert.That(difference.Y, Is.EqualTo(2.5f));
    }

    [Test]
    public void ImplicitConversion_ToVector2_ShouldWork()
    {
        var position = new Position(3, 4, 0.2f, 0.8f);
        Vector2 vector = position;

        Assert.That(vector.X, Is.EqualTo(3.2f));
        Assert.That(vector.Y, Is.EqualTo(4.8f));
    }

    [Test]
    public void ImplicitConversion_FromVector2_ShouldWork()
    {
        var vector = new Vector2(2.7f, -1.3f);
        Position position = vector;

        Assert.That(position.X, Is.EqualTo(2));
        Assert.That(position.Y, Is.EqualTo(-2));
        Assert.That(position.OffsetX, Is.EqualTo(0.7f).Within(0.001f));
        Assert.That(position.OffsetY, Is.EqualTo(0.7f).Within(0.001f));
    }

    [Test]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var pos1 = new Position(2, 3, 0.5f, 0.25f);
        var pos2 = new Position(2, 3, 0.5f, 0.25f);

        Assert.That(pos1.Equals(pos2), Is.True);
        Assert.That(pos1 == pos2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        var pos1 = new Position(2, 3, 0.5f, 0.25f);
        var pos2 = new Position(2, 3, 0.5f, 0.26f);

        Assert.That(pos1.Equals(pos2), Is.False);
        Assert.That(pos1 != pos2, Is.True);
    }

    [Test]
    public void GetHashCode_ForEqualPositions_ShouldBeSame()
    {
        var pos1 = new Position(1, 2, 0.3f, 0.4f);
        var pos2 = new Position(1, 2, 0.3f, 0.4f);

        Assert.That(pos1.GetHashCode(), Is.EqualTo(pos2.GetHashCode()));
    }

    [Test]
    public void ToString_ShouldReturnFormattedString()
    {
        var position = new Position(5, -2, 0.25f, 0.75f);
        var result = position.ToString();

        // Account for different decimal separators in different cultures
        Assert.That(result, Does.Contain("Position(5, -2,"));
        Assert.That(result, Does.Contain("25,"));
        Assert.That(result, Does.Contain("75)"));
    }

    [Test]
    public void Constructor_WithZeroVector_ShouldHandleCorrectly()
    {
        var position = new Position(Vector2.Zero);

        Assert.That(position.X, Is.EqualTo(0));
        Assert.That(position.Y, Is.EqualTo(0));
        Assert.That(position.OffsetX, Is.EqualTo(0f));
        Assert.That(position.OffsetY, Is.EqualTo(0f));
    }

    [Test]
    public void Move_WithLargeNegativeOffset_ShouldHandleCorrectly()
    {
        var position = new Position(5, 5);
        var moved = position.Move(-6.7f, -8.3f);

        // Total after move: (5 - 6.7, 5 - 8.3) = (-1.7, -3.3)
        Assert.That(moved.X, Is.EqualTo(-2)); // Floor of -1.7
        Assert.That(moved.Y, Is.EqualTo(-4)); // Floor of -3.3
        Assert.That(moved.OffsetX, Is.EqualTo(0.3f).Within(0.001f)); // -1.7 - (-2)
        Assert.That(moved.OffsetY, Is.EqualTo(0.7f).Within(0.001f)); // -3.3 - (-4)
    }

    [Test]
    public void Equals_WithObjectOfDifferentType_ShouldReturnFalse()
    {
        var position = new Position(1, 2);
        var notPosition = "not a position";

        Assert.That(position.Equals(notPosition), Is.False);
    }
}
