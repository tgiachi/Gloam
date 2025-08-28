using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Gloam.Console.Render.Platform;
using Gloam.Core.Input;
using Gloam.Core.Input.Base;
using Gloam.Core.Types;
using Serilog;

namespace Gloam.Console.Render.Input;

/// <summary>
///     Console-based input device that reads from the standard console input
/// </summary>
public sealed partial class ConsoleInputDevice : BaseInputDevice, IDisposable
{
    [GeneratedRegex(@"\x1b\[\<(?<btn>\d+);(?<x>\d+);(?<y>\d+)(?<type>[Mm])", RegexOptions.Compiled)]
    private static partial Regex SgrRegex();

    private readonly HashSet<ConsoleKey> _currentlyPressed;
    private readonly Queue<ConsoleKeyInfo> _keyBuffer;

    private readonly ConcurrentQueue<MouseState> _mouseQueue = new();

    private readonly CancellationTokenSource _cts = new();

    private Thread? _mouseThread;
    private bool _mouseEnabled;

    private const string ESC = "\e[";

    private static readonly Regex SgrMouse = SgrRegex();


    /// <summary>
    ///     Initializes a new console input device
    /// </summary>
    public ConsoleInputDevice()
    {
        _keyBuffer = new Queue<ConsoleKeyInfo>();
        _currentlyPressed = new HashSet<ConsoleKey>();

        try
        {
            WinVT.EnableVT();
        }
        catch
        {
            /* ignore */
        }

        EnableMouse();
        StartMouseReader();
    }

    /// <inheritdoc />
    public override void Poll()
    {
        // Clear previous frame's pressed state
        _currentlyPressed.Clear();

        // Read all available keys from console input buffer
        while (System.Console.KeyAvailable)
        {
            try
            {
                var keyInfo = System.Console.ReadKey(true);
                _keyBuffer.Enqueue(keyInfo);
                _currentlyPressed.Add(keyInfo.Key);
            }
            catch
            {
                // Ignore input errors (e.g., when input is redirected)
                break;
            }
        }
    }

    private void EnableMouse()
    {
        if (_mouseEnabled)
        {
            return;
        }

        // xterm mouse + SGR extended
        System.Console.Write($"{ESC}?1000h"); // click
        System.Console.Write($"{ESC}?1002h"); // drag
        System.Console.Write($"{ESC}?1003h"); // any-motion
        System.Console.Write($"{ESC}?1006h"); // SGR extended coords
        _mouseEnabled = true;
    }

    private void MouseReadLoop()
    {
        var stdin = System.Console.OpenStandardInput();
        var buf = new byte[256];
        var sb = new StringBuilder();

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                int n = stdin.Read(buf, 0, buf.Length);
                if (n <= 0)
                {
                    continue;
                }

                sb.Append(Encoding.UTF8.GetString(buf, 0, n));

                var text = sb.ToString();
                int lastConsumed = 0;

                foreach (Match m in SgrMouse.Matches(text))
                {
                    var btnCode = int.Parse(m.Groups["btn"].Value);
                    var x = int.Parse(m.Groups["x"].Value);
                    var y = int.Parse(m.Groups["y"].Value);
                    var typeCh = m.Groups["type"].Value; // "M" press/drag, "m" release

                    var (button, move, shift, alt, ctrl) = DecodeButton(btnCode);

                    var me = new MouseState(
                        X: x,
                        Y: y,
                        Button: button,
                        Pressed: typeCh == "M",
                        Shift: shift,
                        Alt: alt,
                        Ctrl: ctrl,
                        Move: move
                    );

                    _mouseQueue.Enqueue(me);
                    Mouse = me;
                    lastConsumed = m.Index + m.Length;
                }

                if (lastConsumed > 0)
                {
                    sb.Remove(0, lastConsumed);
                }
                else if (sb.Length > 4096)
                {
                    sb.Clear();
                }
            }
        }
        catch
        {
            // Ignore input errors (e.g., when input is redirected)
        }
    }

    // xterm SGR button decoding:
    // btn = base + modifiers, dove:
    //   base 0=Left, 1=Middle, 2=Right, 64=WheelUp, 65=WheelDown
    //   +32 => mouse move
    //   +4  => Shift, +8 => Alt, +16 => Ctrl
    private static (MouseButtonType button, bool move, bool shift, bool alt, bool ctrl)
        DecodeButton(int code)
    {
        bool shift = (code & 4) != 0;
        bool alt = (code & 8) != 0;
        bool ctrl = (code & 16) != 0;
        bool move = (code & 32) != 0;

        int baseCode = code & ~(4 | 8 | 16 | 32);

        MouseButtonType button = baseCode switch
        {
            0  => MouseButtonType.Left,
            1  => MouseButtonType.Middle,
            2  => MouseButtonType.Right,
            64 => MouseButtonType.WheelUp,
            65 => MouseButtonType.WheelDown,
            _  => MouseButtonType.None
        };

        return (button, move, shift, alt, ctrl);
    }

    private void StartMouseReader()
    {
        _mouseThread = new Thread(MouseReadLoop)
        {
            IsBackground = true,
            Name = "Gloam.MouseReader"
        };
        _mouseThread.Start();
    }

    private void DisableMouse()
    {
        if (!_mouseEnabled)
        {
            return;
        }

        System.Console.Write($"{ESC}?1006l");
        System.Console.Write($"{ESC}?1003l");
        System.Console.Write($"{ESC}?1002l");
        System.Console.Write($"{ESC}?1000l");
        _mouseEnabled = false;
    }

    /// <inheritdoc />
    protected override bool GetCurrentKeyState(InputKeyData key)
    {
        // Map Gloam key to ConsoleKey and check if it's currently pressed
        var consoleKey = MapToConsoleKey(key);
        return consoleKey.HasValue && _currentlyPressed.Contains(consoleKey.Value);
    }

    /// <summary>
    ///     Gets the next available key press from the input buffer
    /// </summary>
    /// <returns>The next key press, or null if no keys are available</returns>
    public ConsoleKeyInfo? GetNextKeyPress()
    {
        return _keyBuffer.Count > 0 ? _keyBuffer.Dequeue() : null;
    }

    /// <summary>
    ///     Gets all available key presses from the input buffer
    /// </summary>
    /// <returns>Collection of all available key presses</returns>
    public IEnumerable<ConsoleKeyInfo> GetAllKeyPresses()
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

    private static ConsoleKey? MapToConsoleKey(InputKeyData key)
    {
        // Map common key codes to ConsoleKey
        return key.KeyCode switch
        {
            // Letters (A-Z are virtual key codes 65-90)
            65 => ConsoleKey.A,
            66 => ConsoleKey.B,
            67 => ConsoleKey.C,
            68 => ConsoleKey.D,
            69 => ConsoleKey.E,
            70 => ConsoleKey.F,
            71 => ConsoleKey.G,
            72 => ConsoleKey.H,
            73 => ConsoleKey.I,
            74 => ConsoleKey.J,
            75 => ConsoleKey.K,
            76 => ConsoleKey.L,
            77 => ConsoleKey.M,
            78 => ConsoleKey.N,
            79 => ConsoleKey.O,
            80 => ConsoleKey.P,
            81 => ConsoleKey.Q,
            82 => ConsoleKey.R,
            83 => ConsoleKey.S,
            84 => ConsoleKey.T,
            85 => ConsoleKey.U,
            86 => ConsoleKey.V,
            87 => ConsoleKey.W,
            88 => ConsoleKey.X,
            89 => ConsoleKey.Y,
            90 => ConsoleKey.Z,

            // Numbers (0-9 are virtual key codes 48-57)
            48 => ConsoleKey.D0,
            49 => ConsoleKey.D1,
            50 => ConsoleKey.D2,
            51 => ConsoleKey.D3,
            52 => ConsoleKey.D4,
            53 => ConsoleKey.D5,
            54 => ConsoleKey.D6,
            55 => ConsoleKey.D7,
            56 => ConsoleKey.D8,
            57 => ConsoleKey.D9,

            // Special keys
            13 => ConsoleKey.Enter,     // Keys.Enter
            27 => ConsoleKey.Escape,    // Keys.Escape
            32 => ConsoleKey.Spacebar,  // Keys.Space
            9  => ConsoleKey.Tab,       // Keys.Tab
            8  => ConsoleKey.Backspace, // Keys.Backspace
            46 => ConsoleKey.Delete,    // Keys.Delete

            // Arrow keys
            37 => ConsoleKey.LeftArrow,  // Keys.LeftArrow
            39 => ConsoleKey.RightArrow, // Keys.RightArrow
            38 => ConsoleKey.UpArrow,    // Keys.UpArrow
            40 => ConsoleKey.DownArrow,  // Keys.DownArrow

            // Function keys
            112 => ConsoleKey.F1,
            113 => ConsoleKey.F2,
            114 => ConsoleKey.F3,
            115 => ConsoleKey.F4,
            116 => ConsoleKey.F5,
            117 => ConsoleKey.F6,
            118 => ConsoleKey.F7,
            119 => ConsoleKey.F8,
            120 => ConsoleKey.F9,
            121 => ConsoleKey.F10,
            122 => ConsoleKey.F11,
            123 => ConsoleKey.F12,

            _ => null
        };
    }

    public bool TryDequeueMouseEvent(out MouseState me) => _mouseQueue.TryDequeue(out me);

    public IEnumerable<MouseState> DequeueAllMouseEvents()
    {
        while (_mouseQueue.TryDequeue(out var me)) yield return me;
    }

    public void Dispose()
    {
        DisableMouse();
    }
}
