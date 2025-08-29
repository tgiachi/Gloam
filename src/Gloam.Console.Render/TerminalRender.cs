using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Core.Types;
using Terminal.Gui;
using Color = Gloam.Core.Primitives.Color;

namespace Gloam.Console.Render;

/// <summary>
///     Unified terminal renderer and input device implementation
/// </summary>
public class TerminalRender : Window, IRenderer, IRenderSurface, IInputDevice
{

    private readonly HashSet<InputKeyData> _currentKeys = new();
    private readonly HashSet<InputKeyData> _previousKeys = new();
    private readonly HashSet<InputKeyData> _pressedThisFrame = new();
    private readonly HashSet<InputKeyData> _releasedThisFrame = new();


    public int Width => Application.Screen.Width;
    public int Height => Application.Screen.Height;
    public RenderUnit Unit => RenderUnit.Cells;
    public int CellWidth => 1;

    public int CellHeight => 1;

    public event Action<int, int>? Resized;
    public MouseState Mouse { get; }

    public IRenderSurface Surface => this;

    public TerminalRender()
    {
        Application.SizeChanging += (sender, args) => { Resized?.Invoke(args.Size.Value.Width, args.Size.Value.Height); };
        Application.MouseEvent += ApplicationOnMouseEvent;
        Application.KeyDown += ApplicationOnKeyDown;
        Application.KeyUp += ApplicationOnKeyUp;

    }

    private void ApplicationOnKeyUp(object? sender, Key e)
    {

    }

    private void ApplicationOnKeyDown(object? sender, Key e)
    {

    }

    private void ApplicationOnMouseEvent(object? sender, MouseEventArgs e)
    {

    }

    public void BeginDraw()
    {
    }

    public void DrawText(Position pos, string text, Color fg, Color? bg = null)
    {
        var foreground = new Terminal.Gui.Color(fg.R, fg.G, fg.B);
        var background = bg.HasValue
            ? new Terminal.Gui.Color(bg.Value.R, bg.Value.G, bg.Value.B)
            : Terminal.Gui.Color.Black;

        Move(pos.X, pos.Y);
        Driver.SetAttribute(new Terminal.Gui.Attribute(foreground, background));
        AddStr(text);
        Driver.SetAttribute(new Terminal.Gui.Attribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Black));
    }

    public void DrawTile(Position pos, TileVisual v)
    {
        var foreground = new Terminal.Gui.Color(v.Foreground.R, v.Foreground.G, v.Foreground.B);
        var background = v.Background.HasValue
            ? new Terminal.Gui.Color(v.Background.Value.R, v.Background.Value.G, v.Background.Value.B)
            : Terminal.Gui.Color.Black;

        Move(pos.X, pos.Y);
        Driver.SetAttribute(new Terminal.Gui.Attribute(foreground, background));
        AddRune(v.Glyph);
        Driver.SetAttribute(new Terminal.Gui.Attribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Black));



    }

    public void EndDraw()
    {
    }


    public void Poll()
    {
        _pressedThisFrame.Clear();
        _releasedThisFrame.Clear();

        _previousKeys.Clear();
        foreach (var key in _currentKeys)
        {
            _previousKeys.Add(key);
        }

        // UpdateCurrentKeys(); // Implementa questa funzione per leggere l'input

        // Calcola pressed/released
        foreach (var key in _currentKeys)
        {
            if (!_previousKeys.Contains(key))
            {
                _pressedThisFrame.Add(key);
            }
        }

        foreach (var key in _previousKeys)
        {
            if (!_currentKeys.Contains(key))
            {
                _releasedThisFrame.Add(key);
            }
        }
    }

    public void EndFrame()
    {
    }

    public bool IsDown(InputKeyData key)
    {
        return _currentKeys.Contains(key);
    }

    public bool WasPressed(InputKeyData key)
    {
        return _pressedThisFrame.Contains(key);
    }

    public bool WasReleased(InputKeyData key)
    {
        return _releasedThisFrame.Contains(key);
    }
}
