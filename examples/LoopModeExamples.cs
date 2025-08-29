using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;

/// <summary>
/// Examples demonstrating how to use the new loop management features in GloamHost
/// </summary>
public static class LoopModeExamples
{
    /// <summary>
    /// Example 1: Traditional internal loop mode (default behavior)
    /// The host manages the entire game loop internally
    /// </summary>
    public static async Task InternalLoopExample(GloamHost host)
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true, // Run indefinitely
            LoopMode = LoopMode.Internal, // Default mode
            RenderStep = TimeSpan.FromMilliseconds(33), // ~30 FPS
            SleepTime = TimeSpan.FromMilliseconds(5)
        };

        // This will run the complete game loop internally
        await host.RunAsync(config);
    }

    /// <summary>
    /// Example 2: External loop mode with manual control
    /// The caller has full control over when loop iterations occur
    /// </summary>
    public static async Task ExternalLoopExample(GloamHost host)
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            RenderStep = TimeSpan.FromMilliseconds(16), // ~60 FPS
            HandleTimingInExternalMode = false // We handle timing ourselves
        };

        // Start the host (but don't run the loop yet)
        await host.StartAsync();

        // Manual loop control
        var running = true;
        var frameCount = 0;

        while (running && frameCount < 1000) // Run for 1000 frames
        {
            // Execute one loop iteration
            await host.LoopAsync(config);

            // Custom logic between frames
            frameCount++;
            if (frameCount % 100 == 0)
            {
                Console.WriteLine($"Frame {frameCount} completed");
            }

            // Custom timing control
            await Task.Delay(16); // ~60 FPS
        }

        await host.StopAsync();
    }

    /// <summary>
    /// Example 3: External loop with automatic timing
    /// The host handles timing but the caller controls loop execution
    /// </summary>
    public static async Task ExternalLoopWithTimingExample(GloamHost host)
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            RenderStep = TimeSpan.FromMilliseconds(33), // ~30 FPS
            HandleTimingInExternalMode = true, // Host handles timing
            SleepTime = TimeSpan.FromMilliseconds(5)
        };

        await host.StartAsync();

        var running = true;
        var frameCount = 0;

        while (running && frameCount < 500)
        {
            // Execute one loop iteration (host handles timing internally)
            await host.LoopAsync(config);

            frameCount++;

            // Check loop state for debugging
            var (isFirstFrame, timeSinceStart, timeSinceLastRender) = host.GetLoopState();
            if (frameCount % 60 == 0) // Every ~2 seconds at 30 FPS
            {
                Console.WriteLine($"Frame {frameCount}: Time since start: {timeSinceStart.TotalSeconds:F2}s");
            }
        }

        await host.StopAsync();
    }

    /// <summary>
    /// Example 4: Switching between loop modes
    /// Demonstrates how to switch between internal and external modes
    /// </summary>
    public static async Task ModeSwitchingExample(GloamHost host)
    {
        // Start with internal mode
        var internalConfig = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.Internal,
            RenderStep = TimeSpan.FromMilliseconds(33),
            SleepTime = TimeSpan.FromMilliseconds(5)
        };

        // Run internal loop for a short time
        var cts = new CancellationTokenSource();
        cts.CancelAfter(2000); // Run for 2 seconds

        try
        {
            await host.RunAsync(internalConfig, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled
        }

        // Reset loop state before switching modes
        host.ResetLoopState();

        // Switch to external mode
        var externalConfig = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            RenderStep = TimeSpan.FromMilliseconds(16),
            HandleTimingInExternalMode = true
        };

        await host.StartAsync();

        // Run external loop for another 2 seconds
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < 2)
        {
            await host.LoopAsync(externalConfig);
        }

        await host.StopAsync();
    }

    /// <summary>
    /// Example 5: Advanced external loop with custom frame rate control
    /// Demonstrates precise frame rate control in external mode
    /// </summary>
    public static async Task CustomFrameRateExample(GloamHost host)
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            RenderStep = TimeSpan.FromMilliseconds(10), // Very fast rendering
            HandleTimingInExternalMode = false // We control timing
        };

        await host.StartAsync();

        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / 10); // 10 FPS
        var frameCount = 0;
        var startTime = DateTime.Now;

        while (frameCount < 50) // Run for 50 frames
        {
            var frameStart = DateTime.Now;

            // Execute loop iteration
            await host.LoopAsync(config);

            frameCount++;

            // Calculate and apply precise timing
            var frameTime = DateTime.Now - frameStart;
            var remainingTime = targetFrameTime - frameTime;

            if (remainingTime > TimeSpan.Zero)
            {
                await Task.Delay(remainingTime);
            }

            // Log performance
            if (frameCount % 10 == 0)
            {
                var actualFps = frameCount / (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine($"Frame {frameCount}: Actual FPS: {actualFps:F2}");
            }
        }

        await host.StopAsync();
    }
}