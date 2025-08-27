using System.ComponentModel.DataAnnotations;
using Gloam.Data.Entities.Base;

namespace Gloam.Data.Entities.Colors;

/// <summary>
///     Represents a collection of named colors for use in the game engine.
/// </summary>
public class ColorSetEntity : BaseGloamEntity
{
    /// <summary>
    ///     Gets or sets a dictionary mapping color names to their hex color codes.
    ///     Example: "white" -> "#FFFFFF"
    /// </summary>
    [Required]
    public Dictionary<string, string> Colors { get; set; } = new();
}
