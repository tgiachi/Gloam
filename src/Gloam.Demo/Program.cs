using Gloam.Console.Render.Input;
using Gloam.Console.Render.Layers;
using Gloam.Console.Render.Rendering;
using Gloam.Console.Render.Surfaces;
using Gloam.Demo.Scenes;
using Gloam.Core.Input;
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;

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
        System.Console.WriteLine("Press ESC to exit");
        System.Console.WriteLine("Press any key to start...");
        System.Console.WriteLine("Current size is " + System.Console.WindowWidth + "x" + System.Console.WindowHeight);
        
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
        
        System.Console.ReadKey();

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

            // Setup console rendering
            var surface = new ConsoleSurface();
            var renderer = new ConsoleRenderer(surface);
            var inputDevice = new ConsoleInputDevice();

            host.SetRenderer(renderer);
            host.SetInputDevice(inputDevice);


            // Initialize the host
            await host.InitializeAsync();

            // Setup scene system
            var sceneManager = host.SceneManager;
            
            // Add global layers (always visible)
            sceneManager.AddGlobalLayer(new StatusLayerRenderer());
            sceneManager.AddGlobalLayer(new TransitionLayer(sceneManager));
            
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
            
            // Start with main menu
            await sceneManager.SwitchToSceneAsync("MainMenu");

            // Configure game loop for demo
            var gameLoopConfig = new GameLoopConfig
            {
                KeepRunning = () => !ShouldExit(inputDevice),
                RenderStep = TimeSpan.FromMilliseconds(16), // ~60 FPS
                SleepTime = TimeSpan.FromMilliseconds(10)   // Light CPU usage
            };

            // Start the demo
            System.Console.Clear();
            System.Console.WriteLine("Demo running! Press ESC to exit...");
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
            System.Console.ReadKey();
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
    private static bool ShouldExit(ConsoleInputDevice inputDevice)
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
