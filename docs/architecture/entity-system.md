# Entity System

This document describes Gloam's data-driven entity system, which uses JSON definitions with schema validation for flexible game object management.

## Overview

The entity system is the core of Gloam's data-driven architecture, allowing game objects to be defined in JSON with automatic validation and inheritance support.

## Entity Structure

### Base Entity Format

All entities inherit from a common structure:

```json
{
  "id": "unique_entity_id",
  "name": "Human-readable name",
  "description": "Optional description",
  "visual": {
    "glyph": "@",
    "foreground": "#FFD700",
    "background": null
  },
  "properties": {
    // Entity-specific properties
  }
}
```

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique identifier for the entity |
| `name` | string | Human-readable display name |
| `visual` | object | Visual representation (glyph, colors) |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `description` | string | Detailed description of the entity |
| `properties` | object | Custom properties specific to entity type |

## Visual System

### Glyph Representation

```json
{
  "visual": {
    "glyph": "@",
    "foreground": "#FFD700",
    "background": "#000000",
    "style": "Bold"
  }
}
```

### Color System

Gloam supports multiple color formats:

```json
{
  "visual": {
    "foreground": "#FFD700",    // Hex RGB
    "background": "Gold",       // Named color
    "foregroundRgb": [255, 215, 0]  // RGB array
  }
}
```

### Text Styles

```json
{
  "visual": {
    "style": "Bold",
    "styles": ["Bold", "Underline"]
  }
}
```

## Entity Types

### Player Character

```json
{
  "id": "player",
  "name": "Player Character",
  "description": "The brave adventurer",
  "visual": {
    "glyph": "@",
    "foreground": "#FFD700"
  },
  "properties": {
    "health": 100,
    "maxHealth": 100,
    "mana": 50,
    "maxMana": 50,
    "level": 1,
    "experience": 0,
    "strength": 10,
    "dexterity": 10,
    "intelligence": 10,
    "inventory": [],
    "equipment": {}
  }
}
```

### Monster/NPC

```json
{
  "id": "goblin",
  "name": "Goblin",
  "description": "A small, green-skinned creature",
  "visual": {
    "glyph": "g",
    "foreground": "#00FF00"
  },
  "properties": {
    "health": 25,
    "maxHealth": 25,
    "damage": 5,
    "defense": 2,
    "experienceValue": 10,
    "ai": "aggressive",
    "lootTable": ["gold_coin", "dagger"]
  }
}
```

### Item

```json
{
  "id": "health_potion",
  "name": "Health Potion",
  "description": "Restores 50 health points",
  "visual": {
    "glyph": "!",
    "foreground": "#FF0000"
  },
  "properties": {
    "type": "consumable",
    "rarity": "common",
    "value": 25,
    "effects": [
      {
        "type": "heal",
        "amount": 50
      }
    ],
    "stackable": true,
    "maxStack": 10
  }
}
```

### Terrain

```json
{
  "id": "wall",
  "name": "Stone Wall",
  "description": "An impassable stone wall",
  "visual": {
    "glyph": "#",
    "foreground": "#808080"
  },
  "properties": {
    "type": "terrain",
    "passable": false,
    "transparent": false,
    "destructible": true,
    "hardness": 10
  }
}
```

## Schema Validation

### JSON Schema Generation

Gloam automatically generates JSON schemas from C# entity classes:

```csharp
[JsonSerializable(typeof(PlayerEntity))]
[JsonSerializable(typeof(MonsterEntity))]
[JsonSerializable(typeof(ItemEntity))]
public partial class EntityJsonContext : JsonSerializerContext
{
}
```

### Runtime Validation

```csharp
public async Task<EntityValidationResult> ValidateEntityAsync(string jsonContent)
{
    var entity = JsonSerializer.Deserialize<Entity>(jsonContent, _jsonOptions);

    // Schema validation
    var schemaResult = await _schemaValidator.ValidateAsync(jsonContent, entity.GetType());

    // Custom validation
    var customResult = await ValidateCustomRulesAsync(entity);

    return new EntityValidationResult
    {
        IsValid = schemaResult.IsValid && customResult.IsValid,
        Errors = schemaResult.Errors.Concat(customResult.Errors).ToList()
    };
}
```

### Validation Rules

#### Type Validation
- Ensures correct data types for all properties
- Validates enum values against defined constants
- Checks array bounds and string lengths

#### Range Validation
```json
{
  "properties": {
    "health": {
      "type": "integer",
      "minimum": 0,
      "maximum": 1000
    },
    "level": {
      "type": "integer",
      "minimum": 1,
      "maximum": 100
    }
  }
}
```

#### Required Properties
```json
{
  "required": ["id", "name", "visual"],
  "properties": {
    "id": { "type": "string", "minLength": 1 },
    "name": { "type": "string", "minLength": 1 }
  }
}
```

## Entity Inheritance

### Base Entity Classes

```csharp
public abstract class BaseGloamEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntityVisual Visual { get; set; } = new();
}

public class PlayerEntity : BaseGloamEntity
{
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int Level { get; set; }
    public List<string> Inventory { get; set; } = new();
}

public class MonsterEntity : BaseGloamEntity
{
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Defense { get; set; }
    public string AI { get; set; } = "passive";
}
```

### Inheritance in JSON

```json
{
  "baseEntity": "monster",
  "id": "goblin_warrior",
  "name": "Goblin Warrior",
  "properties": {
    "health": 50,
    "damage": 12,
    "defense": 5,
    "ai": "aggressive"
  }
}
```

## Content Loading

### File Structure

```
content/
├── entities/
│   ├── players/
│   │   ├── warrior.json
│   │   └── mage.json
│   ├── monsters/
│   │   ├── goblin.json
│   │   └── dragon.json
│   └── items/
│       ├── weapons/
│       └── potions/
├── schemas/
│   ├── player.schema.json
│   ├── monster.schema.json
│   └── item.schema.json
└── templates/
    ├── entity_templates.json
    └── item_templates.json
```

### Loading Process

```csharp
public async Task<Entity> LoadEntityAsync(string entityId)
{
    // Find entity file
    string entityPath = FindEntityFile(entityId);

    // Load JSON content
    string jsonContent = await File.ReadAllTextAsync(entityPath);

    // Validate against schema
    var validationResult = await ValidateEntityAsync(jsonContent);
    if (!validationResult.IsValid)
    {
        throw new EntityValidationException(validationResult.Errors);
    }

    // Deserialize entity
    var entity = JsonSerializer.Deserialize<Entity>(jsonContent, _jsonOptions);

    // Apply inheritance if specified
    if (!string.IsNullOrEmpty(entity.BaseEntity))
    {
        entity = await ApplyInheritanceAsync(entity);
    }

    // Cache for future use
    _entityCache[entityId] = entity;

    return entity;
}
```

## Entity Management

### Entity Registry

```csharp
public class EntityRegistry
{
    private readonly Dictionary<string, Entity> _entities = new();
    private readonly Dictionary<Type, List<Entity>> _entitiesByType = new();

    public void RegisterEntity(Entity entity)
    {
        _entities[entity.Id] = entity;

        var entityType = entity.GetType();
        if (!_entitiesByType.ContainsKey(entityType))
        {
            _entitiesByType[entityType] = new List<Entity>();
        }
        _entitiesByType[entityType].Add(entity);
    }

    public Entity GetEntity(string id) => _entities[id];
    public IEnumerable<Entity> GetEntitiesByType(Type type) => _entitiesByType[type];
}
```

### Runtime Entity Creation

```csharp
public class EntityFactory
{
    private readonly EntityRegistry _registry;
    private readonly IServiceProvider _services;

    public T CreateEntity<T>(string entityId) where T : Entity
    {
        var template = _registry.GetEntity(entityId);

        // Clone template
        var entity = JsonSerializer.Deserialize<T>(
            JsonSerializer.Serialize(template),
            _jsonOptions
        );

        // Initialize runtime components
        entity.Initialize(_services);

        return entity;
    }
}
```

## Performance Optimizations

### Entity Caching

```csharp
public class EntityCache
{
    private readonly Dictionary<string, Entity> _cache = new();
    private readonly Dictionary<string, DateTime> _lastAccessed = new();

    public Entity GetOrLoad(string entityId)
    {
        if (_cache.TryGetValue(entityId, out var entity))
        {
            _lastAccessed[entityId] = DateTime.UtcNow;
            return entity;
        }

        entity = LoadEntity(entityId);
        _cache[entityId] = entity;
        _lastAccessed[entityId] = DateTime.UtcNow;

        return entity;
    }

    public void CleanupStaleEntities(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        var staleIds = _lastAccessed
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in staleIds)
        {
            _cache.Remove(id);
            _lastAccessed.Remove(id);
        }
    }
}
```

### Batch Loading

```csharp
public async Task<Dictionary<string, Entity>> LoadEntitiesAsync(IEnumerable<string> entityIds)
{
    var tasks = entityIds.Select(id => LoadEntityAsync(id));
    var entities = await Task.WhenAll(tasks);

    return entityIds.Zip(entities, (id, entity) => new { id, entity })
                   .ToDictionary(x => x.id, x => x.entity);
}
```

## Error Handling

### Validation Errors

```csharp
public class EntityValidationException : Exception
{
    public List<string> ValidationErrors { get; }

    public EntityValidationException(List<string> errors)
        : base($"Entity validation failed: {string.Join(", ", errors)}")
    {
        ValidationErrors = errors;
    }
}
```

### Loading Errors

```csharp
public class EntityLoadingException : Exception
{
    public string EntityId { get; }
    public string EntityPath { get; }

    public EntityLoadingException(string entityId, string path, Exception innerException)
        : base($"Failed to load entity '{entityId}' from '{path}'", innerException)
    {
        EntityId = entityId;
        EntityPath = path;
    }
}
```

## Best Practices

### Entity Design

1. **Keep entities focused**: Each entity should have a single responsibility
2. **Use inheritance wisely**: Don't create deep inheritance hierarchies
3. **Validate early**: Use schema validation to catch errors during development
4. **Document properties**: Add comments to complex property structures

### Performance

1. **Cache frequently used entities**: Avoid reloading common entities
2. **Use batch loading**: Load related entities together
3. **Minimize inheritance depth**: Shallow hierarchies load faster
4. **Profile loading times**: Monitor and optimize slow-loading entities

### Maintenance

1. **Version schemas**: Track schema changes over time
2. **Test entity loading**: Ensure all entities load correctly
3. **Document entity relationships**: Keep track of entity dependencies
4. **Regular validation**: Run validation checks during development

This entity system provides a flexible, maintainable way to define game objects while ensuring data integrity through schema validation and supporting complex inheritance patterns.