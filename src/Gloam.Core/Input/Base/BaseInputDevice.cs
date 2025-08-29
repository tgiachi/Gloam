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
    
    /// <summary>
    /// Cached key states from the last polling operation
    /// </summary>
    private readonly Dictionary<InputKeyData, bool> _polledKeyStates = new();

    public MouseState Mouse { get; protected set; }

    public virtual void Poll()
    {
        // Clear and repopulate the polled key states
        _polledKeyStates.Clear();
        
        // Let concrete implementations populate the polled states
        PopulatePolledKeyStates(_polledKeyStates);
        
        // Update current states from polled states
        foreach (var kvp in _polledKeyStates)
        {
            _currentKeyStates[kvp.Key] = kvp.Value;
        }
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
        // Use cached state from last polling operation, not live checking
        return _currentKeyStates.GetValueOrDefault(key, false);
    }

    public bool WasPressed(InputKeyData key)
    {
        var isCurrentlyDown = IsDown(key);
        var wasPreviouslyDown = _previousKeyStates.GetValueOrDefault(key, false);

        return isCurrentlyDown && !wasPreviouslyDown;
    }

    public bool WasReleased(InputKeyData key)
    {
        var isCurrentlyDown = IsDown(key);
        var wasPreviouslyDown = _previousKeyStates.GetValueOrDefault(key, false);

        return !isCurrentlyDown && wasPreviouslyDown;
    }

    /// <summary>
    /// Concrete implementations should populate the provided dictionary with current key states
    /// This is called once per frame during Poll()
    /// </summary>
    /// <param name="keyStates">Dictionary to populate with current key states</param>
    protected abstract void PopulatePolledKeyStates(Dictionary<InputKeyData, bool> keyStates);

    /// <summary>
    /// Legacy method for backward compatibility - now uses cached states
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key is currently pressed</returns>
    [Obsolete("Use IsDown instead - this method is for internal use only")]
    protected virtual bool GetCurrentKeyState(InputKeyData key)
    {
        return _currentKeyStates.GetValueOrDefault(key, false);
    }
}
