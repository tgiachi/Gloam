using Gloam.Core.Interfaces;

namespace Gloam.Core.Input.Base;

/// <summary>
///     Base implementation of IInputDevice that handles edge detection logic.
///     Concrete implementations only need to override GetCurrentKeyState.
/// </summary>
public abstract class BaseInputDevice : IInputDevice
{
    private readonly Dictionary<InputKeyData, bool> _currentKeyStates = new();
    private readonly Dictionary<InputKeyData, bool> _previousKeyStates = new();

    public virtual void Poll()
    {
        // This can be overridden by concrete implementations for additional polling logic
    }

    public virtual void EndFrame()
    {
        // Copy current states to previous states for next frame's edge detection
        _previousKeyStates.Clear();
        foreach (var kvp in _currentKeyStates)
        {
            _previousKeyStates[kvp.Key] = kvp.Value;
        }
    }

    public bool IsDown(InputKeyData key)
    {
        var isCurrentlyDown = GetCurrentKeyState(key);
        _currentKeyStates[key] = isCurrentlyDown;
        return isCurrentlyDown;
    }

    public bool WasPressed(InputKeyData key)
    {
        var isCurrentlyDown = IsDown(key); // This also updates _currentKeyStates
        var wasPreviouslyDown = _previousKeyStates.GetValueOrDefault(key, false);

        return isCurrentlyDown && !wasPreviouslyDown;
    }

    public bool WasReleased(InputKeyData key)
    {
        var isCurrentlyDown = IsDown(key); // This also updates _currentKeyStates
        var wasPreviouslyDown = _previousKeyStates.GetValueOrDefault(key, false);

        return !isCurrentlyDown && wasPreviouslyDown;
    }

    /// <summary>
    ///     Abstract method that concrete implementations must override to provide current key state
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key is currently pressed</returns>
    protected abstract bool GetCurrentKeyState(InputKeyData key);
}
