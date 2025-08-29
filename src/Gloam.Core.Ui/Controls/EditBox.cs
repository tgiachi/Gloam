using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;

namespace Gloam.Core.Ui.Controls;

/// <summary>
///     An editable text input control
/// </summary>
public class EditBox : BaseControl
{
    private string _text = string.Empty;
    private int _cursorPosition;
    private bool _isReadOnly;
    private int _maxLength = int.MaxValue;
    private TimeSpan _blinkTimer = TimeSpan.Zero;
    private bool _showCursor = true;
    private readonly TimeSpan _blinkInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    ///     Initializes a new instance of EditBox
    /// </summary>
    /// <param name="position">The position of the edit box</param>
    /// <param name="size">The size of the edit box</param>
    public EditBox(Position position, Size size) : base(position, size)
    {
        Background = Colors.DarkGray;
        Foreground = Colors.White;
        CursorColor = Colors.Yellow;
    }

    /// <summary>
    ///     Initializes a new instance of EditBox with default size
    /// </summary>
    /// <param name="position">The position of the edit box</param>
    public EditBox(Position position) : this(position, new Size(20, 3))
    {
    }

    /// <summary>
    ///     Gets or sets the text content of the edit box
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            var newText = value ?? string.Empty;
            if (newText.Length > MaxLength)
                newText = newText[..MaxLength];

            if (_text != newText)
            {
                _text = newText;
                _cursorPosition = Math.Min(_cursorPosition, _text.Length);
                Invalidate();
                TextChanged?.Invoke(this, _text);
            }
        }
    }

    /// <summary>
    ///     Gets or sets the cursor position within the text
    /// </summary>
    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            var newPosition = Math.Clamp(value, 0, _text.Length);
            if (_cursorPosition != newPosition)
            {
                _cursorPosition = newPosition;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets whether the edit box is read-only
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly != value)
            {
                _isReadOnly = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the maximum length of text allowed
    /// </summary>
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            _maxLength = Math.Max(0, value);
            if (_text.Length > _maxLength)
            {
                Text = _text[.._maxLength];
            }
        }
    }

    /// <summary>
    ///     Gets or sets the placeholder text shown when the edit box is empty
    /// </summary>
    public string PlaceholderText { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the cursor color
    /// </summary>
    public Color CursorColor { get; set; }

    /// <summary>
    ///     Event raised when the text content changes
    /// </summary>
    public event EventHandler<string>? TextChanged;

    /// <summary>
    ///     Event raised when Enter key is pressed
    /// </summary>
    public event EventHandler? EnterPressed;

    /// <inheritdoc />
    protected override void RenderContent(IGuiRenderer renderer)
    {
        var textToDisplay = string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(PlaceholderText) 
            ? PlaceholderText 
            : _text;

        var textColor = string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(PlaceholderText) 
            ? new Color(Foreground.R / 2, Foreground.G / 2, Foreground.B / 2, Foreground.A) 
            : Foreground;

        // Render border
        renderer.DrawRectangle(Position, Size, IsFocused ? Colors.White : Colors.Gray);

        // Calculate text position with padding
        var textPosition = new Position(Position.X + 1, Position.Y + 1);
        var availableWidth = Size.Width - 2;

        // Handle text scrolling if it's wider than the available space
        var displayText = textToDisplay;
        var scrollOffset = 0;

        if (!string.IsNullOrEmpty(displayText) && displayText.Length > availableWidth)
        {
            // Calculate scroll offset to keep cursor visible
            if (_cursorPosition > availableWidth - 1)
            {
                scrollOffset = _cursorPosition - availableWidth + 1;
            }
            displayText = displayText.Length > scrollOffset 
                ? displayText[scrollOffset..Math.Min(displayText.Length, scrollOffset + availableWidth)]
                : string.Empty;
        }

        // Render text
        if (!string.IsNullOrEmpty(displayText))
        {
            renderer.DrawText(displayText, textPosition, textColor);
        }

        // Render cursor if focused and cursor is visible
        if (IsFocused && _showCursor && !IsReadOnly)
        {
            var cursorX = textPosition.X + Math.Max(0, _cursorPosition - scrollOffset);
            if (cursorX < Position.X + Size.Width - 1)
            {
                var cursorPosition = new Position(cursorX, textPosition.Y);
                renderer.DrawText("|", cursorPosition, CursorColor);
            }
        }
    }

    /// <inheritdoc />
    protected override void UpdateContent(IInputDevice inputDevice, TimeSpan deltaTime)
    {
        // Update cursor blink
        _blinkTimer += deltaTime;
        if (_blinkTimer >= _blinkInterval)
        {
            _showCursor = !_showCursor;
            _blinkTimer = TimeSpan.Zero;
            Invalidate();
        }

        if (!IsFocused || IsReadOnly)
            return;

        // Handle text input
        HandleTextInput(inputDevice);

        // Handle navigation keys
        HandleNavigationKeys(inputDevice);

        // Handle special keys
        HandleSpecialKeys(inputDevice);
    }

    private void HandleTextInput(IInputDevice inputDevice)
    {
        // This is a simplified implementation - in a real implementation,
        // you'd want to handle character input events from the input device
        // For now, we'll handle basic ASCII keys
        
        var letterKeys = new[] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
                                Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T,
                                Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z };
        
        foreach (var key in letterKeys)
        {
            if (inputDevice.WasPressed(key))
            {
                var isShift = inputDevice.IsDown(Keys.LeftShift) || inputDevice.IsDown(Keys.RightShift);
                var character = isShift ? key.Name[0] : char.ToLower(key.Name[0], System.Globalization.CultureInfo.InvariantCulture);
                InsertCharacter(character);
            }
        }

        var digitKeys = new[] { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        foreach (var key in digitKeys)
        {
            if (inputDevice.WasPressed(key))
            {
                var character = key.Name[0]; // Get the digit character
                InsertCharacter(character);
            }
        }

        // Handle space
        if (inputDevice.WasPressed(Keys.Space))
        {
            InsertCharacter(' ');
        }
    }

    private void HandleNavigationKeys(IInputDevice inputDevice)
    {
        if (inputDevice.WasPressed(Keys.Left))
        {
            CursorPosition = Math.Max(0, CursorPosition - 1);
        }
        
        if (inputDevice.WasPressed(Keys.Right))
        {
            CursorPosition = Math.Min(_text.Length, CursorPosition + 1);
        }

        if (inputDevice.WasPressed(Keys.Home))
        {
            CursorPosition = 0;
        }

        if (inputDevice.WasPressed(Keys.End))
        {
            CursorPosition = _text.Length;
        }
    }

    private void HandleSpecialKeys(IInputDevice inputDevice)
    {
        if (inputDevice.WasPressed(Keys.Backspace) && _cursorPosition > 0)
        {
            var newText = _text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
            Text = newText;
        }

        if (inputDevice.WasPressed(Keys.Delete) && _cursorPosition < _text.Length)
        {
            var newText = _text.Remove(_cursorPosition, 1);
            Text = newText;
        }

        if (inputDevice.WasPressed(Keys.Enter))
        {
            EnterPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void InsertCharacter(char character)
    {
        if (_text.Length >= MaxLength)
            return;

        var newText = _text.Insert(_cursorPosition, character.ToString());
        _cursorPosition++;
        Text = newText;
    }

    /// <summary>
    ///     Selects all text in the edit box
    /// </summary>
    public void SelectAll()
    {
        CursorPosition = _text.Length;
        Invalidate();
    }

    /// <summary>
    ///     Clears all text from the edit box
    /// </summary>
    public void Clear()
    {
        Text = string.Empty;
    }
}