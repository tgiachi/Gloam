using Gloam.Core.Primitives;

namespace Gloam.Console.Render.Rendering;

/// <summary>
/// Represents the visual data for a single console cell
/// </summary>
internal readonly struct CellData : IEquatable<CellData>
{
    /// <summary>
    /// The character to display
    /// </summary>
    public readonly char Character;
    
    /// <summary>
    /// The foreground color, or null for default
    /// </summary>
    public readonly Color? Foreground;
    
    /// <summary>
    /// The background color, or null for transparent
    /// </summary>
    public readonly Color? Background;
    
    /// <summary>
    /// Indicates if this cell is empty (default state)
    /// </summary>
    public readonly bool IsEmpty;

    /// <summary>
    /// Creates a new CellData with the specified character and colors
    /// </summary>
    /// <param name="character">The character to display</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public CellData(char character, Color? foreground, Color? background)
    {
        Character = character;
        Foreground = foreground;
        Background = background;
        IsEmpty = false;
    }
    
    /// <summary>
    /// Private constructor for creating empty cells
    /// </summary>
    /// <param name="isEmpty">Must be true for empty cells</param>
    private CellData(bool isEmpty)
    {
        Character = '\0';
        Foreground = null;
        Background = null;
        IsEmpty = isEmpty;
    }
    
    /// <summary>
    /// Creates an empty cell
    /// </summary>
    public static CellData Empty => new(true);

    /// <inheritdoc />
    public bool Equals(CellData other)
    {
        return Character == other.Character && 
               Nullable.Equals(Foreground, other.Foreground) && 
               Nullable.Equals(Background, other.Background) &&
               IsEmpty == other.IsEmpty;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CellData other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Character, Foreground, Background, IsEmpty);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(CellData left, CellData right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(CellData left, CellData right)
    {
        return !(left == right);
    }
}