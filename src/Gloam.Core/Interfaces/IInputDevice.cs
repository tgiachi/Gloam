using Gloam.Core.Input;

namespace Gloam.Core.Interfaces;

/// <summary>
///     Interface for input devices that can be implemented by Console, MonoGame, etc.
/// </summary>
public interface IInputDevice
{
    /// <summary>
    ///     Gets the current mouse state including position and button states
    /// </summary>
    MouseState Mouse { get; }

    /// <summary>
    ///     Polls the input device for current state
    /// </summary>
    void Poll();

    /// <summary>
    ///     Called at the end of each frame to update edge detection
    /// </summary>
    void EndFrame();

    /// <summary>
    ///     Check if a key is currently held down
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key is currently pressed</returns>
    bool IsDown(InputKeyData key);

    /// <summary>
    ///     Check if a key was pressed this frame (edge detection)
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key was just pressed</returns>
    bool WasPressed(InputKeyData key);

    /// <summary>
    ///     Check if a key was released this frame (edge detection)
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key was just released</returns>
    bool WasReleased(InputKeyData key);
}
