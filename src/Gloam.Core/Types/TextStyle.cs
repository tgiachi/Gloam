namespace Gloam.Core.Types;

/// <summary>
///     Text styling flags that can be combined for rich text rendering
/// </summary>
[Flags]
public enum TextStyle
{
    /// <summary>No styling applied</summary>
    None = 0,

    /// <summary>Bold text weight</summary>
    Bold = 1,

    /// <summary>Dimmed/faded appearance</summary>
    Dim = 2,

    /// <summary>Underlined text</summary>
    Underline = 4,

    /// <summary>Inverted colors (background/foreground swapped)</summary>
    Invert = 8
}
