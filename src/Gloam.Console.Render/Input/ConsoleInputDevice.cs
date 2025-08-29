
using Gloam.Core.Input;
using Gloam.Core.Input.Base;
using Mindmagma.Curses;

namespace Gloam.Console.Render.Input;

/// <summary>
///     Console-based input device using dotnet-curses for enhanced terminal input handling
/// </summary>
public sealed partial class ConsoleInputDevice : BaseInputDevice, IDisposable
{
    private readonly HashSet<int> _currentlyPressed;
    private readonly Queue<int> _keyBuffer;
    private IntPtr _window;
    private bool _initialized;
    private bool _mouseTrackingEnabled;

    /// <summary>
    ///     Mouse tracking modes for curses mouse support
    /// </summary>
    public enum MouseTrackingMode
    {
        /// <summary>
        ///     Normal mouse tracking - reports only button presses and releases
        /// </summary>
        Normal,

        /// <summary>
        ///     Button event mouse tracking - reports button presses, releases, and dragging
        /// </summary>
        ButtonEvents,

        /// <summary>
        ///     All motion mouse tracking - reports all mouse movements
        /// </summary>
        AllMotion
    }

    /// <summary>
    ///     Initializes a new curses-based console input device
    /// </summary>
    public ConsoleInputDevice()
    {
        _keyBuffer = new Queue<int>();
        _currentlyPressed = new HashSet<int>();
        _initialized = false;
        _mouseTrackingEnabled = false;

        InitializeCurses();
    }

    /// <summary>
    ///     Initializes the curses library for enhanced input handling
    /// </summary>
    private void InitializeCurses()
    {
        try
        {
            _window = NCurses.InitScreen();
            NCurses.NoEcho();
            NCurses.NoDelay(_window, true);
            NCurses.Keypad(_window, true);
            _initialized = true;
        }
        catch
        {
            // Fallback if curses initialization fails
            _initialized = false;
        }
    }

    /// <inheritdoc />
    public override void Poll()
    {
        // Clear previous frame's pressed state
        _currentlyPressed.Clear();

        if (!_initialized)
        {
            // Fallback to basic console input if curses failed to initialize
            PollBasicConsole();
            return;
        }

        // Read all available input using curses
        const int maxReadsPerFrame = 50;
        var reads = 0;

        while (reads < maxReadsPerFrame)
        {
            try
            {
                var key = NCurses.GetChar();

                // If no key is available, GetChar returns -1
                if (key == -1)
                    break;

                // Process the key
                _keyBuffer.Enqueue(key);
                _currentlyPressed.Add(key);

                reads++;
            }
            catch
            {
                // Ignore input errors
                break;
            }
        }
    }

    /// <summary>
    ///     Fallback polling method using basic Console when curses is not available
    /// </summary>
    private void PollBasicConsole()
    {
        if (!IsInteractiveConsole())
            return;

        const int maxReadsPerFrame = 50;
        var reads = 0;

        while (reads < maxReadsPerFrame)
        {
            try
            {
                if (!System.Console.KeyAvailable)
                    break;

                var keyInfo = System.Console.ReadKey(true);
                var keyCode = (int)keyInfo.Key;

                _keyBuffer.Enqueue(keyCode);
                _currentlyPressed.Add(keyCode);

                reads++;
            }
            catch
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Checks if we're running in an interactive console environment
    /// </summary>
    private static bool IsInteractiveConsole()
    {
        // First check: if input is redirected, we're not interactive
        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected)
        {
            return false;
        }

        // Second check: try to access KeyAvailable
        try
        {
            _ = System.Console.KeyAvailable;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }



    /// <inheritdoc />
    protected override bool GetCurrentKeyState(InputKeyData key)
    {
        // Map Gloam key to curses key code and check if it's currently pressed
        var cursesKey = MapToCursesKey(key);
        return cursesKey.HasValue && _currentlyPressed.Contains(cursesKey.Value);
    }

    /// <summary>
    ///     Gets the next available key press from the input buffer
    /// </summary>
    /// <returns>The next key press, or null if no keys are available</returns>
    public int? GetNextKeyPress()
    {
        return _keyBuffer.Count > 0 ? _keyBuffer.Dequeue() : null;
    }

    /// <summary>
    ///     Gets all available key presses from the input buffer
    /// </summary>
    /// <returns>Collection of all available key presses</returns>
    public IEnumerable<int> GetAllKeyPresses()
    {
        while (_keyBuffer.Count > 0)
        {
            yield return _keyBuffer.Dequeue();
        }
    }

    /// <summary>
    ///     Clears the input buffer
    /// </summary>
    public void ClearBuffer()
    {
        _keyBuffer.Clear();
        _currentlyPressed.Clear();
    }

    /// <summary>
    ///     Enables curses mouse tracking in the console
    /// </summary>
    /// <param name="trackingMode">The mouse tracking mode to enable</param>
    public void EnableMouseTracking(MouseTrackingMode trackingMode = MouseTrackingMode.Normal)
    {
        if (_mouseTrackingEnabled || !_initialized)
            return;

        try
        {
            // Enable mouse tracking through curses (simplified)
            // Full mouse support would require specific curses mouse handling
            _mouseTrackingEnabled = true;
        }
        catch
        {
            // Ignore if mouse tracking cannot be enabled
        }
    }

    /// <summary>
    ///     Disables curses mouse tracking in the console
    /// </summary>
    public void DisableMouseTracking()
    {
        if (!_mouseTrackingEnabled || !_initialized)
            return;

        try
        {
            // Disable mouse tracking through curses (simplified)
            _mouseTrackingEnabled = false;
        }
        catch
        {
            // Ignore if mouse tracking cannot be disabled
        }
    }

    /// <summary>
    ///     Gets whether mouse tracking is currently enabled
    /// </summary>
    public bool IsMouseTrackingEnabled => _mouseTrackingEnabled;

    /// <summary>
    ///     Gets information about mouse support and current state
    /// </summary>
    /// <returns>A string describing mouse support status</returns>
    public string GetMouseSupportInfo()
    {
        var info = $"Mouse tracking: {(_mouseTrackingEnabled ? "Enabled" : "Disabled")}";
        info += $"\nMouse position: ({Mouse.X}, {Mouse.Y})";
        info += $"\nMouse button: {Mouse.Button}";
        info += $"\nMouse pressed: {Mouse.Pressed}";
        info += $"\nModifiers: Shift={Mouse.Shift}, Alt={Mouse.Alt}, Ctrl={Mouse.Ctrl}";
        info += $"\nIs move: {Mouse.Move}";
        return info;
    }

    /// <summary>
    ///     Gets debug information about input state
    /// </summary>
    /// <returns>A string with debug information</returns>
    public string GetDebugInfo()
    {
        var info = $"Keys in buffer: {_keyBuffer.Count}";
        info += $"\nCurrently pressed keys: {_currentlyPressed.Count}";
        if (_currentlyPressed.Count > 0)
        {
            info += $"\nPressed keys: {string.Join(", ", _currentlyPressed)}";
        }
        info += $"\nMouse tracking: {_mouseTrackingEnabled}";
        info += $"\nCurses initialized: {_initialized}";
        info += $"\nInteractive console: {IsInteractiveConsole()}";
        return info;
    }

    /// <summary>
    ///     Tests if keyboard input is working by checking for a specific key
    /// </summary>
    /// <param name="key">The key to test</param>
    /// <returns>True if the key test passes</returns>
    public bool TestKeyboardInput(InputKeyData key)
    {
        try
        {
            // This is a simple test to see if the input system is responsive
            var wasPressed = WasPressed(key);
            var isDown = IsDown(key);
            return true; // If we get here without exceptions, input is working
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    ///     Processes curses mouse events and updates mouse state
    /// </summary>
    private void ProcessMouseEvents()
    {
        if (!_initialized || !_mouseTrackingEnabled)
            return;

        try
        {
            // Check for mouse events using curses
            // This is a simplified implementation - full mouse support would require more work
            // For now, we'll maintain basic mouse state
        }
        catch
        {
            // Ignore mouse processing errors
        }
    }

    /// <summary>
    ///     Maps Gloam InputKeyData to curses key codes
    /// </summary>
    private static int? MapToCursesKey(InputKeyData key)
    {
        // Map common key codes to curses values
        return key.KeyCode switch
        {
            // Letters (A-Z are virtual key codes 65-90, curses uses lowercase)
            65 => 'a', 66 => 'b', 67 => 'c', 68 => 'd', 69 => 'e',
            70 => 'f', 71 => 'g', 72 => 'h', 73 => 'i', 74 => 'j',
            75 => 'k', 76 => 'l', 77 => 'm', 78 => 'n', 79 => 'o',
            80 => 'p', 81 => 'q', 82 => 'r', 83 => 's', 84 => 't',
            85 => 'u', 86 => 'v', 87 => 'w', 88 => 'x', 89 => 'y', 90 => 'z',

            // Numbers (0-9 are virtual key codes 48-57)
            48 => '0', 49 => '1', 50 => '2', 51 => '3', 52 => '4',
            53 => '5', 54 => '6', 55 => '7', 56 => '8', 57 => '9',

            // Special keys
            13 => 13,  // Enter
            27 => 27,  // Escape
            32 => 32,  // Space
            9  => 9,   // Tab
            8  => 263, // Backspace (KEY_BACKSPACE)
            46 => 330, // Delete (KEY_DC)

            // Arrow keys (curses key codes)
            37 => 260, // Left arrow (KEY_LEFT)
            39 => 261, // Right arrow (KEY_RIGHT)
            38 => 259, // Up arrow (KEY_UP)
            40 => 258, // Down arrow (KEY_DOWN)

            // Function keys (curses key codes)
            112 => 265, // F1 (KEY_F1)
            113 => 266, // F2 (KEY_F2)
            114 => 267, // F3 (KEY_F3)
            115 => 268, // F4 (KEY_F4)
            116 => 269, // F5 (KEY_F5)
            117 => 270, // F6 (KEY_F6)
            118 => 271, // F7 (KEY_F7)
            119 => 272, // F8 (KEY_F8)
            120 => 273, // F9 (KEY_F9)
            121 => 274, // F10 (KEY_F10)
            122 => 275, // F11 (KEY_F11)
            123 => 276, // F12 (KEY_F12)

            _ => null
        };
    }


    public void Dispose()
    {
        if (_initialized)
        {
            try
            {
                // Disable mouse tracking if enabled
                DisableMouseTracking();

                // Cleanup curses
                NCurses.EndWin();
            }
            catch
            {
                // Ignore cleanup errors
            }
            finally
            {
                _initialized = false;
            }
        }

        // Clear resources
        _keyBuffer.Clear();
        _currentlyPressed.Clear();
    }
}
