# Core Components

This document details the core components of the Gloam engine and their responsibilities.

## GloamHost

The main entry point and service orchestrator for Gloam applications.

### Key Responsibilities
- **Service Container Management**: Manages DryIoc dependency injection container
- **Game Loop Orchestration**: Coordinates the main game loop execution
- **Resource Management**: Handles initialization, cleanup, and disposal
- **Configuration Management**: Applies runtime configuration settings

### Key Methods
```csharp
// Initialize the host
await host.InitializeAsync(cancellationToken);

// Run the game loop
await host.RunAsync(gameLoopConfig, cancellationToken);

// Clean up resources
await host.DisposeAsync();
```

### Configuration
```csharp
var config = new GloamHostConfig
{
    RootDirectory = "/path/to/content",
    EnableConsoleLogging = true,
    EnableFileLogging = false,
    LoaderType = ContentLoaderType.FileSystem
};
```

## Game Loop System

Flexible game loop implementation supporting both real-time and turn-based gameplay.

### Loop Modes

#### Internal Mode (Default)
- **Automatic timing**: Built-in frame rate management
- **Thread management**: Handles sleeping between frames
- **Performance monitoring**: Tracks frame timing and performance

#### External Mode
- **Manual control**: Caller manages timing and threading
- **Integration friendly**: Easy integration with existing game loops
- **Performance control**: Full control over timing behavior

### Configuration
```csharp
var loopConfig = new GameLoopConfig
{
    KeepRunning = () => !gameOver,
    RenderStep = TimeSpan.FromMilliseconds(16), // ~60 FPS
    SimulationStep = TimeSpan.Zero, // Turn-based
    SleepTime = TimeSpan.FromMilliseconds(1),
    HandleTimingInExternalMode = false
};
```

## Scene Management

Hierarchical scene system with smooth transitions.

### Scene Lifecycle
1. **Created**: Scene instantiated but not active
2. **Loaded**: Resources loaded and initialized
3. **Active**: Currently receiving updates and rendering
4. **Paused**: Temporarily suspended
5. **Unloaded**: Resources cleaned up

### Scene Transitions
- **FadeTransition**: Smooth alpha blending between scenes
- **PushTransition**: Sliding transition effects
- **Custom Transitions**: Implement `ISceneTransition` for custom effects

### Usage Example
```csharp
// Register scenes
sceneManager.RegisterScene(new MainMenuScene());
sceneManager.RegisterScene(new GameScene());

// Switch with transition
var transition = new FadeTransition(
    TimeSpan.FromSeconds(1),
    FadeDirection.FadeOut
);
await sceneManager.SwitchToSceneAsync("Game", transition);
```

## Entity System

Data-driven entity management with JSON schema validation.

### Entity Structure
```json
{
  "id": "player",
  "name": "Player Character",
  "visual": {
    "glyph": "@",
    "foreground": "#FFD700",
    "background": null
  },
  "stats": {
    "health": 100,
    "mana": 50,
    "strength": 10
  }
}
```

### Key Components

#### BaseGloamEntity
Abstract base class for all game entities:
- **Id**: Unique identifier
- **Name**: Human-readable name
- **Visual**: Rendering information
- **Custom Properties**: Entity-specific data

#### EntityDataLoader
Handles loading and validation of entity definitions:
- **JSON Schema Validation**: Runtime validation against schemas
- **Inheritance Support**: Entity inheritance hierarchies
- **Caching**: Loaded entity caching for performance

#### JsonSchemaValidator
Validates entity JSON against predefined schemas:
- **Auto-generated Schemas**: Generated from C# classes
- **Comprehensive Validation**: Type, range, and format checking
- **Detailed Error Messages**: Clear validation failure information

## Input System

Cross-platform input abstraction with edge detection.

### Key Features
- **Edge Detection**: Pressed, released, and held state tracking
- **Modifier Support**: Ctrl, Alt, Shift combinations
- **Multiple Input Sources**: Keyboard, mouse, gamepad support
- **Platform Abstraction**: Consistent API across platforms

### Input Flow
```
Physical Input → Input Device → Key State Tracking → Edge Detection → Game Logic
```

### Usage Example
```csharp
// Check for key press (only true on initial press)
if (inputDevice.IsDown(Keys.W))
{
    player.MoveUp();
}

// Check for continuous hold
if (inputDevice.WasPressed(Keys.Space))
{
    player.Jump();
}
```

## Rendering Pipeline

Layer-based rendering system with priority ordering.

### Layer System
- **Priority-based Rendering**: Layers rendered in order
- **Transparency Support**: Alpha channel handling
- **Performance Optimized**: Minimal allocations during rendering

### Key Components

#### LayerRenderingManager
Orchestrates layer rendering:
- **Layer Registration**: Dynamic layer addition/removal
- **Priority Management**: Automatic sorting by priority
- **Performance Monitoring**: Rendering performance tracking

#### BaseLayerRenderer
Abstract base for layer implementations:
- **Template Method Pattern**: Defines rendering algorithm
- **Extensibility**: Easy to create custom layers
- **Resource Management**: Automatic cleanup

### Built-in Layers
- **World Layer**: Game world rendering
- **Entity Layer**: Character and object rendering
- **UI Layer**: User interface rendering
- **Transition Layer**: Scene transition effects
- **Debug Layer**: Development information overlay

## Dependency Injection

DryIoc-based service management optimized for game development.

### Container Configuration
```csharp
var rules = Rules.Default
    .WithoutThrowOnRegisteringDisposableTransient()
    .WithoutEagerCachingSingletonForFasterAccess()
    .WithTrackingDisposableTransients()
    .WithUseInterpretation()
    .WithoutImplicitCheckForReuseMatchingScope()
    .WithAutoConcreteTypeResolution()
    .WithDefaultReuse(Reuse.Transient);

var container = new Container(rules);
```

### Service Lifetimes
- **Singleton**: Services created once and reused
- **Transient**: New instance created each time
- **Scoped**: Instance per game loop iteration

### Performance Optimizations
- **Interpretation over Compilation**: Faster startup for dynamic scenarios
- **Minimal Reflection**: Reduced runtime overhead
- **Object Pooling**: Reuse of frequently created objects

## Configuration System

Hierarchical configuration management.

### Configuration Hierarchy
1. **Build Configuration** (`Directory.Build.props`)
2. **Host Configuration** (`GloamHostConfig`)
3. **Game Loop Configuration** (`GameLoopConfig`)
4. **Entity Configuration** (JSON files)

### Configuration Sources
- **Code Configuration**: Programmatic setup
- **Environment Variables**: Runtime environment settings
- **Configuration Files**: JSON/XML configuration files
- **Command Line**: Override settings via CLI

## Error Handling and Logging

Comprehensive error handling and diagnostic capabilities.

### Logging System
- **Serilog Integration**: Structured logging with multiple sinks
- **Log Levels**: Trace, Debug, Information, Warning, Error, Fatal
- **Contextual Logging**: Request/scenario-specific logging
- **Performance Logging**: Frame rate and performance metrics

### Error Handling Patterns
- **Graceful Degradation**: Continue operation with reduced functionality
- **Detailed Error Messages**: Clear information for debugging
- **Exception Safety**: Proper cleanup on error conditions
- **Validation**: Input validation with clear error messages

### Diagnostic Tools
- **Performance Counters**: Frame rate, memory usage, GC statistics
- **Debug Overlays**: Visual debugging information
- **Logging Configuration**: Runtime log level adjustment
- **Health Checks**: System health monitoring

This component overview provides the foundation for understanding how Gloam's parts work together to create a cohesive game engine.