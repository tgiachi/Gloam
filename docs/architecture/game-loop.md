# Game Loop Architecture

This document explains Gloam's flexible game loop implementation and timing systems.

## Overview

Gloam's game loop is designed to support both real-time and turn-based gameplay with precise timing control and performance monitoring.

## Loop Modes

### Internal Mode

The default mode where Gloam manages the entire game loop:

```csharp
var config = new GameLoopConfig
{
    KeepRunning = () => !gameOver,
    RenderStep = TimeSpan.FromMilliseconds(16.67), // ~60 FPS
    SimulationStep = TimeSpan.Zero, // Real-time
    SleepTime = TimeSpan.FromMilliseconds(1)
};

await host.RunAsync(config, cancellationToken);
```

#### Key Features
- **Automatic Frame Rate Management**: Maintains target frame rate
- **Built-in Threading**: Handles sleeping between frames
- **Performance Monitoring**: Tracks frame timing and statistics
- **Resource Management**: Automatic cleanup and disposal

### External Mode

For integration with existing game loops or custom timing requirements:

```csharp
var config = new GameLoopConfig
{
    LoopMode = LoopMode.External,
    KeepRunning = () => !gameOver,
    HandleTimingInExternalMode = true
};

// Manual loop control
while (config.KeepRunning())
{
    await host.LoopAsync(config, cancellationToken);
    // Custom timing logic here
}
```

#### Key Features
- **Manual Control**: Full control over timing and threading
- **Integration Friendly**: Easy integration with existing engines
- **Performance Control**: Custom timing and frame rate management
- **Flexibility**: Adapt to any game loop requirements

## Timing System

### High-Precision Timing

Gloam uses `Stopwatch.GetTimestamp()` for high-precision timing:

```csharp
// Get current timestamp
long currentTime = Stopwatch.GetTimestamp();

// Calculate elapsed time
TimeSpan elapsed = Stopwatch.GetElapsedTime(previousTime, currentTime);

// Convert to milliseconds
double milliseconds = elapsed.TotalMilliseconds;
```

#### Advantages
- **Microsecond Precision**: More accurate than `DateTime.Now`
- **Monotonic Clock**: Not affected by system time changes
- **Cross-Platform**: Consistent behavior across operating systems
- **Performance**: Minimal overhead for timestamp operations

### Frame Timing Calculation

```csharp
private void CalculateFrameTiming()
{
    long currentFrame = Stopwatch.GetTimestamp();

    // Time since game start
    _timeSinceStart = Stopwatch.GetElapsedTime(_startTimestamp, currentFrame);

    // Time since last render
    _timeSinceLastRender = Stopwatch.GetElapsedTime(_lastRenderTimestamp, currentFrame);

    // Update for next frame
    _lastRenderTimestamp = currentFrame;
}
```

## Game Loop Implementation

### Internal Loop Structure

```csharp
private async Task RunInternalLoopAsync(GameLoopConfig config, CancellationToken ct)
{
    InitializeTiming();

    while (config.KeepRunning() && !ct.IsCancellationRequested)
    {
        long now = Stopwatch.GetTimestamp();

        // Input processing
        _inputDevice?.Poll();

        // Game logic update
        await UpdateGameLogicAsync(ct);

        // Rendering (if it's time)
        if (ShouldRender(now, config))
        {
            await RenderFrameAsync(now, ct);
        }

        // Frame cleanup
        _inputDevice?.EndFrame();

        // Sleep to maintain frame rate
        await SleepForNextFrame(config, ct);
    }
}
```

### Key Components

#### Input Processing
```csharp
private void ProcessInput()
{
    // Poll for new input events
    _inputDevice?.Poll();

    // Update key states for edge detection
    UpdateKeyStates();

    // Process buffered input events
    ProcessInputBuffer();
}
```

#### Game Logic Update
```csharp
private async Task UpdateGameLogicAsync(CancellationToken ct)
{
    // Update current scene
    await _sceneManager.UpdateCurrentSceneAsync(ct);

    // Update game systems
    UpdatePhysics();
    UpdateAI();
    UpdateAnimations();

    // Handle events and messages
    ProcessGameEvents();
}
```

#### Rendering Pipeline
```csharp
private async Task RenderFrameAsync(long frameTime, CancellationToken ct)
{
    // Begin rendering
    _renderer?.BeginDraw();

    // Render all layers
    await _layerRenderingManager.RenderAllLayersAsync(renderContext, ct);

    // End rendering
    _renderer?.EndDraw();

    // Update frame statistics
    UpdateFrameStatistics(frameTime);
}
```

## Performance Optimization

### Frame Rate Management

#### Target Frame Rate Calculation
```csharp
private TimeSpan CalculateTargetFrameTime(int targetFps)
{
    return TimeSpan.FromSeconds(1.0 / targetFps);
}
```

#### Adaptive Sleep Timing
```csharp
private TimeSpan CalculateSleepTime(TimeSpan targetFrameTime, TimeSpan actualFrameTime)
{
    TimeSpan sleepTime = targetFrameTime - actualFrameTime;

    // Clamp to reasonable bounds
    if (sleepTime < TimeSpan.Zero)
        return TimeSpan.Zero;

    if (sleepTime > MaxSleepTime)
        return MaxSleepTime;

    return sleepTime;
}
```

### Memory Management

#### Object Pooling
```csharp
private class FrameContextPool
{
    private readonly ConcurrentQueue<FrameContext> _pool = new();
    private readonly int _maxSize;

    public FrameContext Get() => _pool.TryDequeue(out var context) ? context : new FrameContext();
    public void Return(FrameContext context) => _pool.Enqueue(context);
}
```

#### Garbage Collection Optimization
- **Struct-based Primitives**: Reduce heap allocations
- **Object Reuse**: Pool frequently created objects
- **Lazy Initialization**: Defer object creation until needed
- **Efficient Collections**: Use appropriate collection types

### CPU Optimization

#### Minimal Allocations in Hot Paths
```csharp
// Avoid allocations in tight loops
public void UpdateHotPath()
{
    // Use structs instead of classes where possible
    Position newPosition = currentPosition + velocity;

    // Reuse existing objects
    UpdateEntity(ref entity, newPosition);
}
```

#### Branch Prediction Optimization
```csharp
// Optimize for common case
public bool ShouldUpdateEntity(Entity entity)
{
    // Most entities are active, so check this first
    if (entity.IsActive)
    {
        return entity.IsVisible && entity.IsInBounds;
    }

    return false;
}
```

## Real-time vs Turn-based Gameplay

### Real-time Mode

```csharp
var config = new GameLoopConfig
{
    KeepRunning = () => true,
    RenderStep = TimeSpan.FromMilliseconds(16.67), // 60 FPS
    SimulationStep = TimeSpan.Zero, // Continuous
    LoopMode = LoopMode.Internal
};
```

#### Characteristics
- **Continuous Updates**: Game state updates every frame
- **Fixed Timestep**: Consistent update intervals
- **Interpolation**: Smooth visual transitions
- **Input Responsiveness**: Immediate input response

### Turn-based Mode

```csharp
var config = new GameLoopConfig
{
    KeepRunning = () => true,
    RenderStep = TimeSpan.FromMilliseconds(100), // 10 FPS for UI
    SimulationStep = TimeSpan.Zero, // Event-driven
    LoopMode = LoopMode.Internal
};
```

#### Characteristics
- **Event-driven Updates**: Updates triggered by player actions
- **Variable Timing**: No fixed update intervals
- **State-based Rendering**: Render when state changes
- **Input Batching**: Process complete input sequences

## Monitoring and Debugging

### Performance Metrics

```csharp
public class FrameStatistics
{
    public double AverageFrameTime { get; private set; }
    public double MinFrameTime { get; private set; }
    public double MaxFrameTime { get; private set; }
    public int FramesPerSecond { get; private set; }
    public long TotalFrames { get; private set; }
}
```

### Debug Visualization

```csharp
public void RenderDebugOverlay()
{
    // Display frame rate
    DrawText($"FPS: {_frameStats.FramesPerSecond}", debugPosition);

    // Display frame time
    DrawText($"Frame Time: {_frameStats.AverageFrameTime:F2}ms", debugPosition + offset);

    // Display memory usage
    DrawText($"Memory: {GC.GetTotalMemory(false) / 1024 / 1024}MB", debugPosition + offset * 2);
}
```

## Configuration Options

### GameLoopConfig Properties

| Property | Description | Default |
|----------|-------------|---------|
| `KeepRunning` | Function that returns true while the game should continue | `() => true` |
| `RenderStep` | Target time between render frames | `TimeSpan.FromMilliseconds(16.67)` |
| `SimulationStep` | Target time between simulation updates | `TimeSpan.Zero` |
| `SleepTime` | Time to sleep between frames | `TimeSpan.FromMilliseconds(1)` |
| `LoopMode` | Internal or external loop control | `LoopMode.Internal` |
| `HandleTimingInExternalMode` | Whether to handle timing in external mode | `false` |

### Runtime Tuning

```csharp
// High-performance real-time game
var highPerfConfig = new GameLoopConfig
{
    RenderStep = TimeSpan.FromMilliseconds(8.33), // 120 FPS
    SleepTime = TimeSpan.Zero, // No sleeping
    KeepRunning = () => true
};

// Turn-based strategy game
var turnBasedConfig = new GameLoopConfig
{
    RenderStep = TimeSpan.FromMilliseconds(100), // 10 FPS UI updates
    SimulationStep = TimeSpan.Zero, // Event-driven
    SleepTime = TimeSpan.FromMilliseconds(10)
};
```

This flexible game loop architecture allows Gloam to support a wide variety of game types while maintaining high performance and precise timing control.