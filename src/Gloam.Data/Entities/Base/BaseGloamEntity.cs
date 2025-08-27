using System.Text.Json.Serialization;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Data.Entities.Base;

/// <summary>
/// Base class for all Gloam entities with common properties and JSON polymorphism support.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TileSetEntity), "tiles")]
[JsonDerivedType(typeof(ColorSetEntity), "colors")]
public class BaseGloamEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the display name of the entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a comment associated with this entity.
    /// </summary>
    [JsonPropertyName("#")]
    public string Comment { get; set; }
    
    /// <summary>
    /// Gets or sets a detailed description of the entity.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets a list of tags associated with this entity for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
