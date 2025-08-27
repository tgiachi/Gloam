using System.Text.Json;
using Gloam.Core.Json;
using Gloam.Data.Context;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Json;

[TestFixture]
public class BaseGloamEntityJsonTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        JsonUtils.RegisterJsonContext(new GloamDataJsonContext());
    }

    [Test]
    public void Serialize_TileSetAsBasePrismaEntity_ShouldIncludeTypeDiscriminator()
    {
        BaseGloamEntity entity = new TileSetEntity
        {
            Id = "polymorphic-test",
            Name = "Polymorphic TileSet",
            Comment = "Testing polymorphic serialization",
            Tiles =
            [
                new TileEntity { Id = "tile-1", Name = "Test Tile", Glyph = "#" }
            ]
        };

        var json = JsonUtils.Serialize(entity);

        Assert.That(json, Contains.Substring("\"type\": \"tiles\""));
        Assert.That(json, Contains.Substring("polymorphic-test"));
        Assert.That(json, Contains.Substring("Polymorphic TileSet"));
        Assert.That(json, Contains.Substring("tile-1"));
    }

    [Test]
    public void Deserialize_WithTypeDiscriminator_ShouldCreateCorrectType()
    {
        var json = """
                   {
                       "type": "tiles",
                       "id": "deserialization-test",
                       "name": "Deserialization Test",
                       "#": "Testing polymorphic deserialization",
                       "description": "A test tileset for polymorphic deserialization",
                       "tags": ["test", "polymorphic"],
                       "tiles": [
                           {
                               "id": "test-tile",
                               "name": "Test Tile",
                               "glyph": "@",
                               "backgroundColor": "#FF0000",
                               "tags": ["test"]
                           }
                       ]
                   }
                   """;

        var entity = JsonUtils.Deserialize<BaseGloamEntity>(json);

        Assert.That(entity, Is.InstanceOf<TileSetEntity>());

        var tileSet = (TileSetEntity)entity;
        Assert.That(tileSet.Id, Is.EqualTo("deserialization-test"));
        Assert.That(tileSet.Name, Is.EqualTo("Deserialization Test"));
        Assert.That(tileSet.Comment, Is.EqualTo("Testing polymorphic deserialization"));
        Assert.That(tileSet.Description, Is.EqualTo("A test tileset for polymorphic deserialization"));
        Assert.That(tileSet.Tags, Has.Count.EqualTo(2));
        Assert.That(tileSet.Tags, Contains.Item("test"));
        Assert.That(tileSet.Tags, Contains.Item("polymorphic"));
        Assert.That(tileSet.Tiles, Has.Count.EqualTo(1));

        var tile = tileSet.Tiles.First();
        Assert.That(tile.Id, Is.EqualTo("test-tile"));
        Assert.That(tile.Name, Is.EqualTo("Test Tile"));
        Assert.That(tile.Glyph, Is.EqualTo("@"));
        Assert.That(tile.BackgroundColor, Is.EqualTo("#FF0000"));
        Assert.That(tile.Tags, Contains.Item("test"));
    }

    [Test]
    public void RoundTrip_PolymorphicSerialization_ShouldPreserveTypeAndData()
    {
        var originalTileSet = new TileSetEntity
        {
            Id = "roundtrip-test",
            Name = "Round Trip Test",
            Comment = "Testing round-trip polymorphic serialization",
            Description = "Ensures data integrity through serialization cycles",
            Tags = ["test", "roundtrip", "polymorphic"],
            Tiles =
            [
                new TileEntity
                {
                    Id = "wall-tile",
                    Name = "Wall",
                    Glyph = "#",
                    BackgroundColor = "#333333",
                    ForegroundColor = "#CCCCCC",
                    Comment = "A wall tile",
                    Tags = ["wall", "solid"]
                },
                new TileEntity
                {
                    Id = "floor-tile",
                    Name = "Floor",
                    Glyph = ".",
                    BackgroundColor = "#666666",
                    ForegroundColor = "#AAAAAA",
                    Comment = "A floor tile",
                    Tags = ["floor", "walkable"]
                }
            ]
        };

        BaseGloamEntity entityBase = originalTileSet;
        var json = JsonUtils.Serialize(entityBase);
        var deserializedEntity = JsonUtils.Deserialize<BaseGloamEntity>(json);

        Assert.That(deserializedEntity, Is.InstanceOf<TileSetEntity>());

        var deserializedTileSet = (TileSetEntity)deserializedEntity;

        Assert.That(deserializedTileSet.Id, Is.EqualTo(originalTileSet.Id));
        Assert.That(deserializedTileSet.Name, Is.EqualTo(originalTileSet.Name));
        Assert.That(deserializedTileSet.Comment, Is.EqualTo(originalTileSet.Comment));
        Assert.That(deserializedTileSet.Description, Is.EqualTo(originalTileSet.Description));
        Assert.That(deserializedTileSet.Tags, Has.Count.EqualTo(originalTileSet.Tags.Count));
        Assert.That(deserializedTileSet.Tiles, Has.Count.EqualTo(originalTileSet.Tiles.Count));

        foreach (var originalTag in originalTileSet.Tags)
        {
            Assert.That(deserializedTileSet.Tags, Contains.Item(originalTag));
        }

        foreach (var originalTile in originalTileSet.Tiles)
        {
            var deserializedTile = deserializedTileSet.Tiles.First(t => t.Id == originalTile.Id);
            Assert.That(deserializedTile.Name, Is.EqualTo(originalTile.Name));
            Assert.That(deserializedTile.Glyph, Is.EqualTo(originalTile.Glyph));
            Assert.That(deserializedTile.BackgroundColor, Is.EqualTo(originalTile.BackgroundColor));
            Assert.That(deserializedTile.ForegroundColor, Is.EqualTo(originalTile.ForegroundColor));
            Assert.That(deserializedTile.Comment, Is.EqualTo(originalTile.Comment));
        }
    }

    [Test]
    public void Serialize_MultiplePolymorphicEntities_ShouldHandleCorrectly()
    {
        var entities = new List<BaseGloamEntity>
        {
            new TileSetEntity
            {
                Id = "set-1",
                Name = "First Set",
                Tiles = [new TileEntity { Id = "tile-1", Glyph = "#" }]
            },
            new TileSetEntity
            {
                Id = "set-2",
                Name = "Second Set",
                Tiles = [new TileEntity { Id = "tile-2", Glyph = "." }]
            }
        };

        var json = JsonUtils.Serialize(entities);
        var deserializedEntities = JsonUtils.Deserialize<List<BaseGloamEntity>>(json);

        Assert.That(deserializedEntities, Has.Count.EqualTo(2));
        Assert.That(deserializedEntities[0], Is.InstanceOf<TileSetEntity>());
        Assert.That(deserializedEntities[1], Is.InstanceOf<TileSetEntity>());

        var firstSet = (TileSetEntity)deserializedEntities[0];
        var secondSet = (TileSetEntity)deserializedEntities[1];

        Assert.That(firstSet.Id, Is.EqualTo("set-1"));
        Assert.That(firstSet.Name, Is.EqualTo("First Set"));
        Assert.That(firstSet.Tiles.First().Glyph, Is.EqualTo("#"));

        Assert.That(secondSet.Id, Is.EqualTo("set-2"));
        Assert.That(secondSet.Name, Is.EqualTo("Second Set"));
        Assert.That(secondSet.Tiles.First().Glyph, Is.EqualTo("."));
    }

    [Test]
    public void Deserialize_InvalidTypeDiscriminator_ShouldThrowException()
    {
        var json = """
                   {
                       "type": "invalid-type",
                       "id": "invalid-test",
                       "name": "Invalid Type Test"
                   }
                   """;

        Assert.Throws<JsonException>(() => JsonUtils.Deserialize<BaseGloamEntity>(json));
    }

    [Test]
    public void Serialize_BasePrismaEntityDirectly_ShouldNotIncludeTypeDiscriminator()
    {
        var baseEntity = new BaseGloamEntity
        {
            Id = "base-test",
            Name = "Base Entity Test",
            Comment = "Testing base entity serialization",
            Tags = ["base", "test"]
        };

        var json = JsonUtils.Serialize(baseEntity);

        Assert.That(json, Does.Not.Contain("\"type\""));
        Assert.That(json, Contains.Substring("base-test"));
        Assert.That(json, Contains.Substring("Base Entity Test"));
    }

    [Test]
    public void CommentProperty_JsonPropertyName_ShouldSerializeAsHashSymbol()
    {
        BaseGloamEntity entity = new TileSetEntity
        {
            Id = "comment-test",
            Comment = "This should serialize as # property"
        };

        var json = JsonUtils.Serialize(entity);

        Assert.That(json, Contains.Substring("\"#\": \"This should serialize as # property\""));
        Assert.That(json, Does.Not.Contain("\"comment\""));
    }
}
