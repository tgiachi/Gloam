# Architecture Overview

This document provides a high-level overview of Gloam's architecture, design principles, and component organization.

## Design Philosophy

Gloam follows several key design principles:

### 1. **Separation of Concerns**
- **Core Framework** (`Gloam.Core`) - Interfaces, primitives, and abstractions
- **Runtime Engine** (`Gloam.Runtime`) - Game loop, dependency injection, and orchestration
- **Data Layer** (`Gloam.Data`) - Entity management and content loading
- **Rendering Backends** (`Gloam.Console.Render`) - Platform-specific implementations
- **UI System** (`Gloam.Core.Ui`) - User interface components

### 2. **Data-Driven Design**
- JSON-based entity definitions with schema validation
- Runtime configuration through structured config objects
- Extensible content loading system

### 3. **Performance-First Approach**
- DryIoc dependency injection optimized for game loops
- Source generation for JSON serialization
- Efficient memory management patterns

## Core Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                 Gloam.Demo                        │    │
│  │              (Example Game)                       │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────┐
│                 Runtime Layer                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                 Gloam.Runtime                       │    │
│  │         Game Loop, DI Container, Services           │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────┐
│                 Abstraction Layer                            │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                 Gloam.Core                         │    │
│  │      Interfaces, Primitives, Base Classes          │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                 Gloam.Data                         │    │
│  │         Entity System, Content Loading             │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │               Gloam.Core.Ui                        │    │
│  │            UI Controls and Components              │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────┐
│              Platform Implementation Layer                  │
│  ┌─────────────────────────────────────────────────────┐    │
│  │           Gloam.Console.Render                      │    │
│  │         Console Rendering, Input, Surfaces          │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Component Breakdown

### Gloam.Core

The foundation layer containing:

- **Interfaces**: Core abstractions (`IRenderer`, `IInputDevice`, `IScene`, etc.)
- **Primitives**: Basic types (`Color`, `Position`, `Size`, `Direction`)
- **Base Classes**: Abstract implementations (`BaseInputDevice`, `BaseTransition`)
- **Utilities**: Helper classes for common operations
- **Types**: Enumerations and constants

### Gloam.Runtime

The orchestration layer providing:

- **GloamHost**: Main entry point and service container
- **Game Loop**: Flexible loop implementation supporting real-time and turn-based games
- **Service Management**: Dependency injection and service lifecycle
- **Scene Management**: Scene transitions and state management
- **Configuration**: Runtime configuration and host settings

### Gloam.Data

The data management layer featuring:

- **Entity System**: JSON-driven entity definitions with inheritance
- **Schema Validation**: Runtime validation using JSON Schema
- **Content Loading**: Extensible loader system for different content types
- **Serialization**: Source-generated JSON serialization for performance

### Gloam.Console.Render

The console rendering backend including:

- **ConsoleRenderer**: Main rendering orchestrator
- **ConsoleSurface**: Console buffer management
- **ConsoleInputDevice**: Input handling and key mapping
- **Layer Rendering**: Priority-based layer system
- **ANSI Color Support**: Rich color rendering

### Gloam.Core.Ui

The user interface system providing:

- **BaseControl**: Abstract base for all UI controls
- **Built-in Controls**: TextBox, ProgressBar, ContainerControl, etc.
- **Layout System**: Flexible positioning and sizing
- **Event Handling**: Input event routing and handling
- **Styling**: Color and appearance customization

## Key Patterns

### 1. **Dependency Injection**
- DryIoc container for service management
- Constructor injection for required dependencies
- Property injection for optional dependencies
- Scoped services for game loop operations

### 2. **Template Method Pattern**
- Abstract base classes define the algorithm structure
- Concrete implementations provide specific behavior
- Example: `BaseInputDevice` and `BaseLayerRenderer`

### 3. **Strategy Pattern**
- Multiple rendering backends (Console, future OpenGL/WebGL)
- Pluggable content loaders
- Configurable transition effects

### 4. **Observer Pattern**
- Event-driven architecture for input handling
- Scene transition notifications
- Rendering pipeline callbacks

### 5. **Factory Pattern**
- Entity creation from JSON definitions
- Service instantiation through DI container
- Renderer backend selection

## Data Flow

```
Input Events → Input Device → Scene Manager → Current Scene
                                      ↓
                                 Update Logic
                                      ↓
                            Layer Rendering Manager
                                      ↓
Renderer Backend → Surface → Display Output
```

## Performance Considerations

### Memory Management
- Object pooling for frequently created objects
- Struct-based primitives to reduce GC pressure
- Efficient buffer management in rendering

### CPU Optimization
- Source generation for JSON serialization
- Optimized game loop with precise timing
- Minimal allocations in hot paths

### I/O Optimization
- Asynchronous content loading
- Caching strategies for frequently used assets
- Lazy initialization of services

## Extensibility Points

### Custom Rendering Backends
1. Implement `IRenderer` interface
2. Create surface implementation
3. Register with dependency injection container

### Custom Content Loaders
1. Implement `IContentLoader` interface
2. Add to `ContentLoaderType` enum
3. Configure in `GloamHostConfig`

### Custom UI Controls
1. Inherit from `BaseControl`
2. Implement required abstract methods
3. Override virtual methods for customization

### Custom Entity Types
1. Inherit from `BaseGloamEntity`
2. Define JSON schema
3. Register with entity loader

## Configuration System

Gloam uses a hierarchical configuration approach:

- **Global Settings**: `Directory.Build.props` for build configuration
- **Host Configuration**: `GloamHostConfig` for runtime settings
- **Game Configuration**: `GameLoopConfig` for loop-specific settings
- **Entity Configuration**: JSON files for game content

## Error Handling

- **Validation**: Schema validation for all JSON entities
- **Graceful Degradation**: Fallback behavior for missing resources
- **Detailed Logging**: Comprehensive logging with Serilog
- **Exception Safety**: Proper cleanup in error scenarios

## Testing Strategy

- **Unit Tests**: Individual component testing
- **Integration Tests**: Component interaction testing
- **Performance Tests**: Benchmarking critical paths
- **Schema Tests**: JSON validation testing

This architecture provides a solid foundation for building roguelike games while maintaining flexibility, performance, and maintainability.