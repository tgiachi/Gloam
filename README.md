# Gloam

**A .NET roguelike game engine with data-driven entity management and flexible rendering.**

Gloam provides a comprehensive foundation for building roguelike games with modern .NET technologies. It features a data-driven architecture where entities, tiles, and game objects are defined in JSON with schema validation, combined with a flexible rendering system that can adapt to different output targets.

## Getting Started

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Use the CLI tool (when implemented)
dotnet run --project tools/Gloam.Cli
```

## Roadmap

### Current Status
- **M0-M2** ✅ Core foundation with data management, JSON schemas, and DryIoc container
- **M3** 🚧 Runtime host and basic entity loading

### Planned Milestones
- **M4**: Entity-Component-System architecture
- **M5**: Console rendering system  
- **M6**: Input handling and game loop
- **M7**: Basic roguelike systems (movement, combat, inventory)
- **M8**: Advanced features (AI, pathfinding, procedural generation)
- **M9**: Polish and optimization

## Architecture

- **Gloam.Core**: Shared utilities, primitives, and JSON handling
- **Gloam.Data**: Entity management, validation, and data loading
- **Gloam.Runtime**: DryIoc-based host and service configuration
- **Gloam.Cli**: Command-line tools for development workflows

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.