namespace Gloam.Core.Primitives;

/// <summary>
/// Provides predefined color constants for common colors
/// </summary>
public static class Colors
{
    // Basic colors
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Yellow = new(255, 255, 0);
    public static readonly Color Magenta = new(255, 0, 255);
    public static readonly Color Cyan = new(0, 255, 255);

    // Dark colors
    public static readonly Color DarkRed = new(128, 0, 0);
    public static readonly Color DarkGreen = new(0, 128, 0);
    public static readonly Color DarkBlue = new(0, 0, 128);
    public static readonly Color DarkYellow = new(128, 128, 0);
    public static readonly Color DarkMagenta = new(128, 0, 128);
    public static readonly Color DarkCyan = new(0, 128, 128);
    public static readonly Color DarkGray = new(64, 64, 64);

    // Light colors
    public static readonly Color LightRed = new(255, 128, 128);
    public static readonly Color LightGreen = new(128, 255, 128);
    public static readonly Color LightBlue = new(128, 128, 255);
    public static readonly Color LightYellow = new(255, 255, 128);
    public static readonly Color LightMagenta = new(255, 128, 255);
    public static readonly Color LightCyan = new(128, 255, 255);
    public static readonly Color LightGray = new(192, 192, 192);
    public static readonly Color Gray = new(128, 128, 128);

    // Orange colors
    public static readonly Color Orange = new(255, 165, 0);
    public static readonly Color DarkOrange = new(255, 140, 0);
    public static readonly Color OrangeRed = new(255, 69, 0);

    // Pink colors
    public static readonly Color Pink = new(255, 192, 203);
    public static readonly Color HotPink = new(255, 105, 180);
    public static readonly Color DeepPink = new(255, 20, 147);

    // Purple colors
    public static readonly Color Purple = new(128, 0, 128);
    public static readonly Color DarkPurple = new(75, 0, 130);
    public static readonly Color Violet = new(238, 130, 238);

    // Brown colors
    public static readonly Color Brown = new(165, 42, 42);
    public static readonly Color DarkBrown = new(101, 67, 33);
    public static readonly Color SaddleBrown = new(139, 69, 19);

    // Gold/Metallic colors
    public static readonly Color Gold = new(255, 215, 0);
    public static readonly Color Silver = new(192, 192, 192);
    public static readonly Color Bronze = new(205, 127, 50);

    // Nature colors
    public static readonly Color ForestGreen = new(34, 139, 34);
    public static readonly Color OliveGreen = new(107, 142, 35);
    public static readonly Color SeaGreen = new(46, 139, 87);
    public static readonly Color SkyBlue = new(135, 206, 235);
    public static readonly Color DeepSkyBlue = new(0, 191, 255);
    public static readonly Color Navy = new(0, 0, 128);
    
    // Transparent color
    public static readonly Color Transparent = new(0, 0, 0, 0);

    // Common UI colors
    public static readonly Color Background = new(32, 32, 32);
    public static readonly Color Foreground = new(224, 224, 224);
    public static readonly Color Accent = new(0, 122, 255);
    public static readonly Color Warning = new(255, 193, 7);
    public static readonly Color Error = new(220, 53, 69);
    public static readonly Color Success = new(40, 167, 69);
    public static readonly Color Info = new(23, 162, 184);

    // Roguelike common colors
    public static readonly Color FloorColor = new(64, 64, 64);
    public static readonly Color WallColor = new(96, 96, 96);
    public static readonly Color PlayerColor = new(255, 255, 0);
    public static readonly Color EnemyColor = new(255, 0, 0);
    public static readonly Color ItemColor = new(0, 255, 255);
    public static readonly Color HealthColor = new(255, 0, 0);
    public static readonly Color ManaColor = new(0, 0, 255);
    public static readonly Color ExperienceColor = new(255, 215, 0);
}