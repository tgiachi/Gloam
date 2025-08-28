using Gloam.Core.Types;

namespace Gloam.Core.Input;

/// <summary>
///     Represents the current state of a mouse including position and button states
/// </summary>
/// <param name="X">The X coordinate in cells</param>
/// <param name="Y">The Y coordinate in cells</param>
public readonly record struct MouseState(
    int X,
    int Y,
    MouseButtonType Button,
    bool Pressed,
    bool Shift,
    bool Alt,
    bool Ctrl,
    bool Move
);
