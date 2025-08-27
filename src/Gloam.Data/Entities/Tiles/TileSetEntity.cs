using System.ComponentModel.DataAnnotations;
using Gloam.Data.Entities.Base;

namespace Gloam.Data.Entities.Tiles;

/// <summary>
///     Represents a collection of tiles grouped together as a tileset.
/// </summary>
public class TileSetEntity : BaseGloamEntity
{
    /// <summary>
    ///     Gets or sets the list of tiles contained in this tileset.
    /// </summary>
    [Required]
    public List<TileEntity> Tiles { get; set; } = new();
}
