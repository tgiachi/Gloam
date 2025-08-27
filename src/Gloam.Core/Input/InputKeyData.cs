namespace Gloam.Core.Input;

/// <summary>
///     Represents input key data that can be implemented by different input systems (Console, MonoGame, etc.)
/// </summary>
public readonly struct InputKeyData : IEquatable<InputKeyData>
{
    /// <summary>
    ///     The key code identifier
    /// </summary>
    public int KeyCode { get; }

    /// <summary>
    ///     Optional modifier flags (Shift, Ctrl, Alt, etc.)
    /// </summary>
    public InputModifiers Modifiers { get; }

    /// <summary>
    ///     Human-readable name for debugging
    /// </summary>
    public string Name { get; }

    public InputKeyData(int keyCode, InputModifiers modifiers = InputModifiers.None, string? name = null)
    {
        KeyCode = keyCode;
        Modifiers = modifiers;
        Name = name ?? keyCode.ToString();
    }

    public InputKeyData(int keyCode, string name) : this(keyCode, InputModifiers.None, name)
    {
    }

    public bool Equals(InputKeyData other)
    {
        return KeyCode == other.KeyCode && Modifiers == other.Modifiers;
    }

    public override bool Equals(object? obj)
    {
        return obj is InputKeyData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(KeyCode, Modifiers);
    }

    public static bool operator ==(InputKeyData left, InputKeyData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(InputKeyData left, InputKeyData right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return Modifiers == InputModifiers.None ? Name : $"{Modifiers}+{Name}";
    }
}
