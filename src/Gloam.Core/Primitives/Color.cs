namespace Gloam.Core.Primitives;

public readonly struct Color
{
    public int R { get; }
    public int G { get; }
    public int B { get; }
    public int A { get; }


    public Color(int r, int g, int b, int a = 255)
    {
        R = Math.Clamp(r, 0, 255);
        G = Math.Clamp(g, 0, 255);
        B = Math.Clamp(b, 0, 255);
        A = Math.Clamp(a, 0, 255);
    }

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

        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        int a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : 255;

        return new Color(r, g, b, a);
    }
}
