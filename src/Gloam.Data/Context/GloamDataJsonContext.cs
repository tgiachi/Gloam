using System.Text.Json.Serialization;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Data.Context;

/// <summary>
///     JSON serialization context for Gloam data entities using source generation for better performance
/// </summary>
[JsonSerializable(typeof(BaseGloamEntity))]
[JsonSerializable(typeof(TileEntity))]
[JsonSerializable(typeof(TileSetEntity))]
[JsonSerializable(typeof(ColorSetEntity))]
[JsonSerializable(typeof(List<TileEntity>))]
[JsonSerializable(typeof(List<BaseGloamEntity>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class GloamDataJsonContext : JsonSerializerContext
{
}
