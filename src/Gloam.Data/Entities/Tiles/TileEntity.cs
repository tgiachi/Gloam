using System.ComponentModel.DataAnnotations;
using Gloam.Data.Entities.Base;

namespace Gloam.Data.Entities.Tiles;

/// <summary>
/// Represents a single tile in the roguelike game engine with visual properties.
/// </summary>
public class TileEntity : BaseGloamEntity
{
    /// <summary>
    /// Gets or sets the character glyph displayed for this tile.
    /// </summary>
    [Required]
    public string Glyph { get; set; }

    /// <summary>
    /// Gets or sets the background color of the tile.
    /// </summary>
    [Required]
    public string BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground color of the tile.
    /// </summary>
    [Required]
    public string ForegroundColor { get; set; }
}
