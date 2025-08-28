namespace Gloam.Core.Input;

/// <summary>
///     Input modifier flags that can be combined
/// </summary>
[Flags]
public enum InputModifiers : byte
{
    /// <summary>No modifier keys pressed</summary>
    None = 0,

    /// <summary>Shift key modifier</summary>
    Shift = 1 << 0,

    /// <summary>Control key modifier</summary>
    Ctrl = 1 << 1,

    /// <summary>Alt key modifier</summary>
    Alt = 1 << 2,

    /// <summary>Meta key modifier (Windows key, Cmd key, etc.)</summary>
    Meta = 1 << 3
}
