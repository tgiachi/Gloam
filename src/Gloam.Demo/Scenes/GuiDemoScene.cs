using Gloam.Console.Render.Input;
using Gloam.Console.Render.Rendering;
using Gloam.Console.Render.Surfaces;
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

    public async ValueTask ReturnToMenuAsync(CancellationToken ct = default)
    {
        if (_sceneManager != null)
        {
            var pushTransition = new PushTransition(TimeSpan.FromMilliseconds(800), PushDirection.FromLeft,
                _sceneManager.CurrentScene, _sceneManager.Scenes["MainMenu"]);
            await _sceneManager.SwitchToSceneAsync("MainMenu", pushTransition, ct);
        }
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
    private bool _initialized = false;
    
    // Demo data
    private int _progressValue = 0;
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
            await _scene.ReturnToMenuAsync(ct);
        }
    }

    private void InitializeGui(RenderLayerContext context)
    {
        // Create console GUI renderer
        var consoleGuiRenderer = new ConsoleGuiRenderer();
        
        // Create the GUI layer renderer
        _guiRenderer = new GuiLayerRenderer(consoleGuiRenderer, priority: 100, name: "GUI Demo Layer");
        _scene.SetGuiLayer(_guiRenderer);

        // Create a simple layout with direct controls
        
        // Info text box
        var infoText = new TextBox(new Position(2, 2), new Size(40, 8))
        {
            Text = "Welcome to the Gloam GUI Demo!\n\n" +
                   "Controls:\n" +
                   "• TAB - Navigate between controls\n" +
                   "• Type in the EditBox below\n" +
                   "• Watch the ProgressBar animate\n" +
                   "• ESC - Return to menu",
            Alignment = TextAlignment.Left,
            WordWrap = true,
            Background = Colors.DarkBlue,
            Foreground = Colors.White
        };
        _guiRenderer.AddControl(infoText);

        // EditBox for text input
        var editBox = new EditBox(new Position(2, 12), new Size(40, 3))
        {
            Text = "",
            PlaceholderText = "Type something here...",
            Background = Colors.DarkGray,
            Foreground = Colors.White,
            MaxLength = 50
        };
        _guiRenderer.AddControl(editBox);

        // ProgressBar
        var progressBar = new ProgressBar(new Position(2, 17), new Size(40, 3))
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            FillColor = Colors.Green,
            BorderColor = Colors.Gray,
            ShowText = true,
            Style = ProgressBarStyle.Continuous
        };
        _guiRenderer.AddControl(progressBar);

        // Container with child controls
        var container = new ContainerControl(new Position(45, 2), new Size(30, 18))
        {
            Background = Colors.DarkRed,
            Foreground = Colors.Yellow
        };
        
        // Add controls to container
        var containerLabel = new TextBox(new Position(1, 1), new Size(28, 2))
        {
            Text = "Container with child controls:",
            Background = Colors.Transparent,
            Foreground = Colors.Yellow,
            WordWrap = true
        };
        
        var containerProgress = new ProgressBar(new Position(1, 4), new Size(28, 3))
        {
            Value = 75,
            FillColor = Colors.Cyan,
            BorderColor = Colors.White,
            ShowText = true,
            CustomText = "Container Progress"
        };

        var containerEdit = new EditBox(new Position(1, 8), new Size(28, 3))
        {
            Text = "Edit inside container",
            Background = Colors.Black,
            Foreground = Colors.Cyan
        };

        container.AddChild(containerLabel);
        container.AddChild(containerProgress);
        container.AddChild(containerEdit);
        
        _guiRenderer.AddControl(container);

        // Set initial focus
        _guiRenderer.SetFocus(editBox);
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
        foreach (var control in _guiRenderer.GetType().GetField("_controls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_guiRenderer) as System.Collections.IEnumerable ?? new object[0])
        {
            if (control is WindowControl window && window.Title == "GUI Controls Demo")
            {
                FindAndUpdateProgressBars(window, _progressValue);
                break;
            }
        }
    }

    private void FindAndUpdateProgressBars(IGuiControl control, int value)
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