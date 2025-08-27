namespace Gloam.Core.Primitives;

/// <summary>
/// Represents a color with red, green, blue, and alpha components (RGBA format)
/// </summary>
public readonly struct Color
{
    /// <summary>
    /// Gets the red component (0-255)
    /// </summary>
    public int R { get; }

    /// <summary>
    /// Gets the green component (0-255)
    /// </summary>
    public int G { get; }

    /// <summary>
    /// Gets the blue component (0-255)
    /// </summary>
    public int B { get; }

    /// <summary>
    /// Gets the alpha (opacity) component (0-255, where 255 is fully opaque)
    /// </summary>
    public int A { get; }

    /// <summary>
    /// Initializes a new Color with the specified RGBA components
    /// </summary>
    /// <param name="r">Red component (0-255, will be clamped)</param>
    /// <param name="g">Green component (0-255, will be clamped)</param>
    /// <param name="b">Blue component (0-255, will be clamped)</param>
    /// <param name="a">Alpha component (0-255, will be clamped). Defaults to 255 (fully opaque)</param>
    public Color(int r, int g, int b, int a = 255)
    {
        R = Math.Clamp(r, 0, 255);
        G = Math.Clamp(g, 0, 255);
        B = Math.Clamp(b, 0, 255);
        A = Math.Clamp(a, 0, 255);
    }

    /// <summary>
    /// Creates a Color from a hexadecimal string representation
    /// </summary>
    /// <param name="hex">Hexadecimal color string (e.g., "#FF0000" or "FF0000" for red, optionally with alpha)</param>
    /// <returns>A Color representing the hexadecimal value</returns>
    /// <exception cref="ArgumentException">Thrown when hex string is invalid</exception>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));
        }

        hex = hex.TrimStart('#');

        if (hex.Length != 6 && hex.Length != 8)
        {
            throw new ArgumentException("Hex string must be 6 or 8 characters long.", nameof(hex));
        }

        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);
        var a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : 255;

        return new Color(r, g, b, a);
    }
}
