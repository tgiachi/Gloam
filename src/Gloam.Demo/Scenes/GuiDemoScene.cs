using Gloam.Console.Render.Input;

using Gloam.Console.Render.Rendering;
using Gloam.Core.Contexts;
using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;
using Gloam.Core.Ui;
using Gloam.Core.Ui.Controls;
using Gloam.Runtime.Services;
using Gloam.Runtime.Transitions;
using Gloam.Runtime.Types;

namespace Gloam.Demo.Scenes;

/// <summary>
///     Demonstration scene showcasing GUI controls
/// </summary>
public sealed class GuiDemoScene : BaseScene
{
    private ISceneManager? _sceneManager;
    private GuiLayerRenderer? _guiLayer;

    public GuiDemoScene() : base("GuiDemo")
    {
        // Add the GUI demonstration layer
        AddLayer(new GuiDemoLayer(this));
    }

    public void SetSceneManager(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public ValueTask ReturnToMenuAsync(CancellationToken ct = default)
    {
        if (_sceneManager != null)
        {
            var pushTransition = new PushTransition(TimeSpan.FromMilliseconds(800), PushDirection.FromLeft,
                _sceneManager.CurrentScene, _sceneManager.Scenes["MainMenu"]);
            
            // Start transition without blocking
            _ = _sceneManager.SwitchToSceneAsync("MainMenu", pushTransition, ct);
        }
        
        return ValueTask.CompletedTask;
    }

    protected override ValueTask ActivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask DeactivateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateSceneAsync(CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public void SetGuiLayer(GuiLayerRenderer guiLayer)
    {
        _guiLayer = guiLayer;
    }
}

/// <summary>
///     Layer that demonstrates GUI controls
/// </summary>
internal sealed class GuiDemoLayer : BaseLayerRenderer
{
    private readonly GuiDemoScene _scene;
    private GuiLayerRenderer? _guiRenderer;
    private bool _initialized;

    // Demo data
    private int _progressValue;
    private DateTime _lastProgressUpdate = DateTime.Now;
    private readonly TimeSpan _progressUpdateInterval = TimeSpan.FromMilliseconds(100);

    public GuiDemoLayer(GuiDemoScene scene)
    {
        _scene = scene;
    }

    public override int Priority => 1000;
    public override string Name => "GUI Demo";

    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Initialize GUI on first render
        if (!_initialized)
        {
            InitializeGui(context);
            _initialized = true;
        }

        // Update progress bar demo
        UpdateProgressBarDemo();

        // Render the GUI layer
        if (_guiRenderer != null)
        {
            await _guiRenderer.RenderAsync(context, ct);
        }

        // Handle menu return input
        if (context.InputDevice.WasPressed(Keys.Escape))
        {
            _ = _scene.ReturnToMenuAsync(ct);
        }
    }

    private void InitializeGui(RenderLayerContext context)
    {
        // Create console GUI renderer
        var consoleGuiRenderer = new ConsoleGuiRenderer();
        
        // Create the GUI layer renderer
        _guiRenderer = new GuiLayerRenderer(consoleGuiRenderer, priority: 100, name: "GUI Demo Layer");
        _scene.SetGuiLayer(_guiRenderer);

        // Create main window
        var mainWindow = new WindowControl(new Position(5, 2), new Size(70, 20), "GUI Controls Demo")
        {
            Background = Colors.DarkBlue,
            Foreground = Colors.White,
            BorderColor = Colors.Cyan,
            TitleColor = Colors.Yellow,
            ShowTitle = true
        };

        // First label and edit box
        var label1 = new TextBox(new Position(2, 2), new Size(25, 1))
        {
            Text = "Name:",
            Background = Colors.Transparent,
            Foreground = Colors.White,
            Alignment = TextAlignment.Left
        };

        var editBox1 = new EditBox(new Position(2, 3), new Size(30, 3))
        {
            Text = "",
            PlaceholderText = "Enter your name...",
            Background = Colors.DarkGray,
            Foreground = Colors.White,
            MaxLength = 25
        };

        // Second label and edit box  
        var label2 = new TextBox(new Position(2, 7), new Size(25, 1))
        {
            Text = "Email:",
            Background = Colors.Transparent,
            Foreground = Colors.White,
            Alignment = TextAlignment.Left
        };

        var editBox2 = new EditBox(new Position(2, 8), new Size(30, 3))
        {
            Text = "",
            PlaceholderText = "Enter your email...",
            Background = Colors.DarkGray,
            Foreground = Colors.White,
            MaxLength = 40
        };

        // Progress bar for demo
        var progressBar = new ProgressBar(new Position(2, 12), new Size(30, 3))
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            FillColor = Colors.Green,
            BorderColor = Colors.Gray,
            ShowText = true,
            Style = ProgressBarStyle.Continuous
        };

        // Instructions
        var instructions = new TextBox(new Position(35, 2), new Size(32, 10))
        {
            Text = "Instructions:\n\n" +
                   "• TAB - Navigate between controls\n" +
                   "• Type in the edit boxes\n" +
                   "• Use Backspace to delete\n" +
                   "• Watch the progress bar\n" +
                   "• ESC - Return to menu",
            Background = Colors.Transparent,
            Foreground = Colors.Yellow,
            WordWrap = true,
            Alignment = TextAlignment.Left
        };

        // Add all controls to the window
        mainWindow.AddChild(label1);
        mainWindow.AddChild(editBox1);
        mainWindow.AddChild(label2);
        mainWindow.AddChild(editBox2);
        mainWindow.AddChild(progressBar);
        mainWindow.AddChild(instructions);

        // Auto-size the window to fit contents
        mainWindow.AutoSizeHeight(15, 3);

        // Add the window to the renderer
        _guiRenderer.AddControl(mainWindow);

        // Set focus to first edit box
        _guiRenderer.SetFocus(editBox1);
    }

    private void UpdateProgressBarDemo()
    {
        if (_guiRenderer == null) return;

        var now = DateTime.Now;
        if (now - _lastProgressUpdate < _progressUpdateInterval) return;

        _lastProgressUpdate = now;
        _progressValue = (_progressValue + 1) % 101; // Cycle from 0 to 100

        // Find and update the progress bar
        // This is a simplified approach - in a real application you'd maintain references
        foreach (var control in _guiRenderer.GetType().GetField("_controls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_guiRenderer) as System.Collections.IEnumerable ?? Array.Empty<object>())
        {
            if (control is WindowControl window && window.Title == "GUI Controls Demo")
            {
                FindAndUpdateProgressBars(window, _progressValue);
                break;
            }
        }
    }

    private static void FindAndUpdateProgressBars(IGuiControl control, int value)
    {
        if (control is ProgressBar progressBar)
        {
            progressBar.Value = value;
        }

        foreach (var child in control.Children)
        {
            FindAndUpdateProgressBars(child, value);
        }
    }
}