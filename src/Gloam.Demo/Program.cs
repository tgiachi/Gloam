using Gloam.Console.Render;
using Gloam.Demo.Scenes;
using Gloam.Core.Input;
using Gloam.Core.Interfaces;
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;
using Terminal.Gui;

namespace Gloam.Demo;

/// <summary>
///     Demo application showcasing Gloam engine with console rendering
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        System.Console.WriteLine("Starting Gloam Console Demo...");
        System.Console.WriteLine("This is a minimal demo showing console integration");
        System.Console.WriteLine("Controls:");
        System.Console.WriteLine("  • Keyboard: WASD/Arrows for movement, ESC to exit");
        System.Console.WriteLine("  • Mouse: Click to move player (if terminal supports VT100)");
        System.Console.WriteLine("Press any key to start...");
        System.Console.WriteLine("Current size is " + System.Console.WindowWidth + "x" + System.Console.WindowHeight);

        // Add debug information
        System.Console.WriteLine("Debug Info:");
        System.Console.WriteLine($"  Terminal: {System.Console.WindowWidth}x{System.Console.WindowHeight}");
        System.Console.WriteLine($"  Interactive: {Environment.UserInteractive}");
        System.Console.WriteLine($"  Input redirected: {System.Console.IsInputRedirected}");
        System.Console.WriteLine($"  Output redirected: {System.Console.IsOutputRedirected}");
        System.Console.WriteLine();

        // Check VT100/24-bit color support
        var supportsVT100 = DetectTrueColorSupport();
        System.Console.WriteLine($"VT100/24-bit colors: {(supportsVT100 ? "✓ Supported" : "✗ Not supported")}");
        if (supportsVT100)
        {
            System.Console.WriteLine("  → Enhanced color rendering enabled");
        }
        else
        {
            System.Console.WriteLine("  → Using legacy ConsoleColor fallback");
        }

        // Wait for user input more safely
        try
        {
            System.Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // If ReadKey fails (e.g., in non-interactive environments), wait a bit
            System.Console.WriteLine("Non-interactive environment detected. Starting demo in 2 seconds...");
            await Task.Delay(2000);
        }

        try
        {
            // Setup console for optimal rendering
            System.Console.Clear();
            System.Console.CursorVisible = false;

            // Configure the game host
            var hostConfig = new GloamHostConfig
            {
                RootDirectory = Directory.GetCurrentDirectory(),
                LoaderType = ContentLoaderType.FileSystem,
                LogLevel = LogLevelType.Information,
                EnableConsoleLogging = false, // Disable logging to avoid interfering with rendering
                EnableFileLogging = true
            };

            // Create and configure the host
            await using var host = new GloamHost(hostConfig);

            // Setup unified terminal rendering
            var terminalRender = new TerminalRender();

            host.SetRenderer(terminalRender);
            host.SetInputDevice(terminalRender);


            // Initialize the host
            await host.InitializeAsync();

            // Setup scene system
            var sceneManager = host.SceneManager;

            // Global layers removed - will be re-implemented with Terminal.Gui
            // sceneManager.AddGlobalLayer(new StatusLayerRenderer());
            // sceneManager.AddGlobalLayer(new TransitionLayer(sceneManager));

            // Register scenes
            var mainMenuScene = new MainMenuScene();
            mainMenuScene.SetSceneManager(sceneManager);
            sceneManager.RegisterScene(mainMenuScene);

            var gameScene = new GameScene();
            gameScene.SetSceneManager(sceneManager);
            sceneManager.RegisterScene(gameScene);

            var flameScene = new FlameScene();
            flameScene.SetSceneManager(sceneManager);
            sceneManager.RegisterScene(flameScene);

            var guiDemoScene = new GuiDemoScene();
            guiDemoScene.SetSceneManager(sceneManager);
            sceneManager.RegisterScene(guiDemoScene);

            // Start with main menu
            await sceneManager.SwitchToSceneAsync("MainMenu");

            // Configure game loop for demo
            var gameLoopConfig = new GameLoopConfig
            {
                KeepRunning = () => !ShouldExit(terminalRender),
                RenderStep = TimeSpan.FromMilliseconds(16), // ~60 FPS
                SleepTime = TimeSpan.FromMilliseconds(10)   // Light CPU usage
            };

            // Start the demo
            System.Console.Clear();
            System.Console.WriteLine("Demo running! Press ESC to exit...");

            Application.Run(terminalRender);
            await host.RunAsync(gameLoopConfig);


            return 0;
        }
        catch (Exception ex)
        {
            System.Console.Clear();
            System.Console.CursorVisible = true;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Demo failed with error: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Console.ResetColor();
            System.Console.WriteLine("Press any key to exit...");
            try
            {
                if (!System.Console.IsInputRedirected)
                {
                    System.Console.ReadKey();
                }
                else
                {
                    // If input is redirected, wait a bit before exiting
                    System.Threading.Tasks.Task.Delay(2000).Wait();
                }
            }
            catch
            {
                // If ReadKey fails for any reason, just wait a bit
                System.Threading.Tasks.Task.Delay(2000).Wait();
            }
            return 1;
        }
        finally
        {
            // Restore console state
            System.Console.Clear();
            System.Console.CursorVisible = true;
            System.Console.ResetColor();
            System.Console.WriteLine("Demo ended.");
        }
    }

    /// <summary>
    ///     Checks if the user wants to exit the demo
    /// </summary>
    private static bool ShouldExit(IInputDevice inputDevice)
    {
        // Check for ESC key press
        return inputDevice.WasPressed(Keys.Escape);
    }

    /// <summary>
    /// Detects if the current terminal supports 24-bit True Color
    /// </summary>
    /// <returns>True if 24-bit color is supported</returns>
    private static bool DetectTrueColorSupport()
    {
        // Check common environment variables that indicate 24-bit color support
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (!string.IsNullOrEmpty(colorTerm))
        {
            var colorTermLower = colorTerm.ToLowerInvariant();
            if (colorTermLower.Contains("truecolor") || colorTermLower.Contains("24bit"))
            {
                return true;
            }
        }

        // Check TERM variable for terminals known to support 24-bit color
        var term = Environment.GetEnvironmentVariable("TERM");
        if (!string.IsNullOrEmpty(term))
        {
            var termLower = term.ToLowerInvariant();
            return termLower.Contains("256color") ||
                   termLower.Contains("truecolor") ||
                   termLower.StartsWith("xterm-") ||
                   termLower.StartsWith("screen-") ||
                   termLower == "tmux" ||
                   termLower == "alacritty" ||
                   termLower == "kitty";
        }

        // On Windows, modern Windows Terminal and Windows 10+ console support 24-bit
        if (OperatingSystem.IsWindows())
        {
            return Environment.OSVersion.Version.Major >= 10;
        }

        // Default to false for unknown terminals
        return false;
    }
}
