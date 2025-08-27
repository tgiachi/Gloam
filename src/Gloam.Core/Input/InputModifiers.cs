namespace Gloam.Core.Input;

/// <summary>
///     Input modifier flags that can be combined
/// </summary>
[Flags]
public enum InputModifiers : byte
{
    None = 0,
    Shift = 1 << 0,
    Ctrl = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3 // Windows key, Cmd key, etc.
}
