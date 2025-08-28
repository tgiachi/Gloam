namespace Gloam.Core.Input;

/// <summary>
///     Represents the current state of a mouse including position and button states
/// </summary>
/// <param name="X">The X coordinate in cells</param>
/// <param name="Y">The Y coordinate in cells</param>
/// <param name="LeftDown">True if the left mouse button is currently pressed</param>
/// <param name="RightDown">True if the right mouse button is currently pressed</param>
/// <param name="MiddleDown">True if the middle mouse button is currently pressed</param>
public readonly record struct MouseState(
    int X,
    int Y,
    bool LeftDown,
    bool RightDown,
    bool MiddleDown
);
