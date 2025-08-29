using Gloam.Core.Primitives;

namespace Gloam.Core.Ui.Controls;

/// <summary>
///     A progress bar control that displays completion progress
/// </summary>
public class ProgressBar : BaseControl
{
    private double _value;
    private double _minimum;
    private double _maximum = 100;
    private bool _showText = true;
    private string _customText = string.Empty;

    /// <summary>
    ///     Initializes a new instance of ProgressBar
    /// </summary>
    /// <param name="position">The position of the progress bar</param>
    /// <param name="size">The size of the progress bar</param>
    public ProgressBar(Position position, Size size) : base(position, size)
    {
        Background = Colors.DarkGray;
        Foreground = Colors.White;
        FillColor = Colors.Green;
        BorderColor = Colors.Gray;
    }

    /// <summary>
    ///     Initializes a new instance of ProgressBar with default size
    /// </summary>
    /// <param name="position">The position of the progress bar</param>
    public ProgressBar(Position position) : this(position, new Size(30, 3))
    {
    }

    /// <summary>
    ///     Gets or sets the current value of the progress bar
    /// </summary>
    public double Value
    {
        get => _value;
        set
        {
            var newValue = Math.Clamp(value, _minimum, _maximum);
            if (Math.Abs(_value - newValue) > double.Epsilon)
            {
                _value = newValue;
                Invalidate();
                ValueChanged?.Invoke(this, _value);
            }
        }
    }

    /// <summary>
    ///     Gets or sets the minimum value of the progress bar
    /// </summary>
    public double Minimum
    {
        get => _minimum;
        set
        {
            if (Math.Abs(_minimum - value) > double.Epsilon)
            {
                _minimum = value;
                if (_maximum < _minimum)
                    _maximum = _minimum;
                
                Value = Math.Max(_value, _minimum);
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the maximum value of the progress bar
    /// </summary>
    public double Maximum
    {
        get => _maximum;
        set
        {
            if (Math.Abs(_maximum - value) > double.Epsilon)
            {
                _maximum = value;
                if (_minimum > _maximum)
                    _minimum = _maximum;
                
                Value = Math.Min(_value, _maximum);
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets the percentage completion (0-100)
    /// </summary>
    public double Percentage
    {
        get
        {
            var range = _maximum - _minimum;
            return range <= 0 ? 0 : (_value - _minimum) / range * 100;
        }
    }

    /// <summary>
    ///     Gets or sets whether to show percentage text on the progress bar
    /// </summary>
    public bool ShowText
    {
        get => _showText;
        set
        {
            if (_showText != value)
            {
                _showText = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets custom text to display instead of percentage
    /// </summary>
    public string CustomText
    {
        get => _customText;
        set
        {
            var newText = value ?? string.Empty;
            if (_customText != newText)
            {
                _customText = newText;
                Invalidate();
            }
        }
    }

    /// <summary>
    ///     Gets or sets the fill color for the progress portion
    /// </summary>
    public Color FillColor { get; set; }

    /// <summary>
    ///     Gets or sets the border color
    /// </summary>
    public Color BorderColor { get; set; }

    /// <summary>
    ///     Gets or sets the style of the progress bar
    /// </summary>
    public ProgressBarStyle Style { get; set; } = ProgressBarStyle.Continuous;

    /// <summary>
    ///     Event raised when the value changes
    /// </summary>
    public event EventHandler<double>? ValueChanged;

    /// <inheritdoc />
    protected override void RenderContent(IGuiRenderer renderer)
    {
        // Render border (highlight if focused)
        var borderColor = IsFocused ? Colors.White : BorderColor;
        renderer.DrawRectangle(Position, Size, borderColor);

        // Calculate fill area
        var fillArea = new Rectangle(
            new Position(Position.X + 1, Position.Y + 1),
            new Size(Size.Width - 2, Size.Height - 2)
        );

        if (fillArea.Width <= 0 || fillArea.Height <= 0)
            return;

        // Render background fill area
        renderer.FillRectangle(new Position(fillArea.X, fillArea.Y), new Size(fillArea.Width, fillArea.Height), Background);

        // Calculate progress width
        var range = _maximum - _minimum;
        var progress = range <= 0 ? 0 : (_value - _minimum) / range;
        var fillWidth = Math.Max(0, (int)(fillArea.Width * progress));

        if (fillWidth > 0)
        {
            var fillRect = new Rectangle(
                fillArea.X,
                fillArea.Y,
                fillWidth,
                fillArea.Height
            );

            switch (Style)
            {
                case ProgressBarStyle.Continuous:
                    renderer.FillRectangle(new Position(fillRect.X, fillRect.Y), new Size(fillRect.Width, fillRect.Height), FillColor);
                    break;
                    
                case ProgressBarStyle.Blocks:
                    RenderBlocks(renderer, fillRect, fillArea);
                    break;
            }
        }

        // Render text if enabled
        if (_showText && Size.Height >= 1)
        {
            var text = string.IsNullOrEmpty(_customText) 
                ? $"{Percentage:F0}%" 
                : _customText;

            if (!string.IsNullOrEmpty(text))
            {
                var textPosition = new Position(
                    Position.X + Math.Max(0, (Size.Width - text.Length) / 2),
                    Position.Y + Size.Height / 2
                );

                renderer.DrawText(text, textPosition, Foreground);
            }
        }
    }

    private void RenderBlocks(IGuiRenderer renderer, Rectangle fillRect, Rectangle totalArea)
    {
        var blockWidth = Math.Max(1, totalArea.Width / 10); // 10 blocks max
        var blockCount = fillRect.Width / blockWidth;
        
        for (var i = 0; i < blockCount; i++)
        {
            var blockX = fillRect.X + (i * blockWidth);
            var blockSize = new Size(Math.Min(blockWidth - 1, fillRect.Right - blockX), fillRect.Height);
            
            if (blockSize.Width > 0)
            {
                renderer.FillRectangle(
                    new Position(blockX, fillRect.Y),
                    blockSize,
                    FillColor
                );
            }
        }
    }

    /// <summary>
    ///     Sets the progress bar to indeterminate mode with animation
    /// </summary>
    public void SetIndeterminate()
    {
        // This would require animation state in a full implementation
        Style = ProgressBarStyle.Marquee;
        Invalidate();
    }

    /// <summary>
    ///     Sets the range of the progress bar
    /// </summary>
    /// <param name="minimum">The minimum value</param>
    /// <param name="maximum">The maximum value</param>
    public void SetRange(double minimum, double maximum)
    {
        _minimum = minimum;
        _maximum = Math.Max(minimum, maximum);
        Value = Math.Clamp(_value, _minimum, _maximum);
        Invalidate();
    }

    /// <summary>
    ///     Increments the progress bar value by the specified amount
    /// </summary>
    /// <param name="amount">The amount to increment</param>
    public void Increment(double amount = 1)
    {
        Value += amount;
    }

    /// <summary>
    ///     Resets the progress bar to its minimum value
    /// </summary>
    public void Reset()
    {
        Value = Minimum;
    }
}

/// <summary>
///     Defines the visual style of the progress bar
/// </summary>
public enum ProgressBarStyle
{
    /// <summary>Continuous fill style</summary>
    Continuous,
    /// <summary>Discrete blocks style</summary>
    Blocks,
    /// <summary>Marquee/indeterminate style</summary>
    Marquee
}