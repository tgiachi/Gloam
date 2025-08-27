using Gloam.Core.Json;
using Gloam.Data.Context;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Json.Tiles;

[TestFixture]
public class TileSetEntityJsonTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        JsonUtils.RegisterJsonContext(new GloamDataJsonContext());
    }

    [Test]
    public void Serialize_EmptyTileSet_ShouldProduceValidJson()
    {
        var tileSet = new TileSetEntity
        {
            Id = "tileset-001",
            Name = "Empty Tileset",
            Comment = "A tileset with no tiles",
            Description = "Used for testing empty tilesets",
            Tags = ["test", "empty"]
        };

        var json = JsonUtils.Serialize(tileSet);

        Assert.That(json, Is.Not.Null.And.Not.Empty);
        Assert.That(json, Contains.Substring("tileset-001"));
        Assert.That(json, Contains.Substring("Empty Tileset"));
        Assert.That(json, Contains.Substring("\"tiles\": []"));
        Assert.That(json, Contains.Substring("test"));
    }

    [Test]
    public void Serialize_TileSetWithTiles_ShouldIncludeAllTiles()
    {
        var wallTile = new TileEntity
        {
            Id = "wall",
            Name = "Wall",
            Glyph = "#",
            BackgroundColor = "#333333"
        };

        var floorTile = new TileEntity
        {
            Id = "floor",
            Name = "Floor",
            Glyph = ".",
            BackgroundColor = "#666666"
        };

        var tileSet = new TileSetEntity
        {
            Id = "basic-set",
            Name = "Basic Tileset",
            Tiles = [wallTile, floorTile]
        };

        var json = JsonUtils.Serialize(tileSet);

        Assert.That(json, Contains.Substring("basic-set"));
        Assert.That(json, Contains.Substring("Basic Tileset"));
        Assert.That(json, Contains.Substring("wall"));
        Assert.That(json, Contains.Substring("floor"));
        Assert.That(json, Contains.Substring("#333333"));
        Assert.That(json, Contains.Substring("#666666"));
    }

    [Test]
    public void Deserialize_ShouldRecreateCompleteObject()
    {
        var originalTileSet = new TileSetEntity
        {
            Id = "test-set",
            Name = "Test Tileset",
            Comment = "For testing",
            Description = "A tileset used in unit tests",
            Tags = ["test", "unit"],
            Tiles =
            [
                new TileEntity
                {
                    Id = "test-wall",
                    Name = "Test Wall",
                    Glyph = "#",
                    Tags = ["wall"]
                },
                new TileEntity
                {
                    Id = "test-floor",
                    Name = "Test Floor",
                    Glyph = ".",
                    Tags = ["floor"]
                }
            ]
        };

        var json = JsonUtils.Serialize(originalTileSet);
        var deserializedTileSet = JsonUtils.Deserialize<TileSetEntity>(json);

        Assert.That(deserializedTileSet, Is.Not.Null);
        Assert.That(deserializedTileSet.Id, Is.EqualTo(originalTileSet.Id));
        Assert.That(deserializedTileSet.Name, Is.EqualTo(originalTileSet.Name));
        Assert.That(deserializedTileSet.Comment, Is.EqualTo(originalTileSet.Comment));
        Assert.That(deserializedTileSet.Description, Is.EqualTo(originalTileSet.Description));
        Assert.That(deserializedTileSet.Tags, Is.EqualTo(originalTileSet.Tags));
        Assert.That(deserializedTileSet.Tiles, Has.Count.EqualTo(2));

        var wallTile = deserializedTileSet.Tiles.First(t => t.Id == "test-wall");
        Assert.That(wallTile.Name, Is.EqualTo("Test Wall"));
        Assert.That(wallTile.Glyph, Is.EqualTo("#"));
        Assert.That(wallTile.Tags, Contains.Item("wall"));

        var floorTile = deserializedTileSet.Tiles.First(t => t.Id == "test-floor");
        Assert.That(floorTile.Name, Is.EqualTo("Test Floor"));
        Assert.That(floorTile.Glyph, Is.EqualTo("."));
        Assert.That(floorTile.Tags, Contains.Item("floor"));
    }

    [Test]
    public void Serialize_WithNullProperties_ShouldHandleGracefully()
    {
        var tileSet = new TileSetEntity
        {
            Id = "minimal-set",
            Name = "Minimal Set"
        };

        var json = JsonUtils.Serialize(tileSet);
        var deserializedTileSet = JsonUtils.Deserialize<TileSetEntity>(json);

        Assert.That(deserializedTileSet.Id, Is.EqualTo("minimal-set"));
        Assert.That(deserializedTileSet.Name, Is.EqualTo("Minimal Set"));
        Assert.That(deserializedTileSet.Comment, Is.Null);
        Assert.That(deserializedTileSet.Description, Is.Null);
        Assert.That(deserializedTileSet.Tags, Is.Not.Null.And.Empty);
        Assert.That(deserializedTileSet.Tiles, Is.Not.Null.And.Empty);
    }

    [Test]
    public void Deserialize_WithMissingTiles_ShouldInitializeEmptyList()
    {
        var json = """
                   {
                       "id": "no-tiles-set",
                       "name": "No Tiles Set"
                   }
                   """;

        var tileSet = JsonUtils.Deserialize<TileSetEntity>(json);

        Assert.That(tileSet.Id, Is.EqualTo("no-tiles-set"));
        Assert.That(tileSet.Name, Is.EqualTo("No Tiles Set"));
        Assert.That(tileSet.Tiles, Is.Not.Null.And.Empty);
    }

    [Test]
    public void RoundTrip_ComplexTileSet_ShouldPreserveAllData()
    {
        var originalTileSet = new TileSetEntity
        {
            Id = "dungeon-set-001",
            Name = "Dungeon Tileset",
            Comment = "Classic dungeon tiles",
            Description = "A comprehensive set of tiles for dungeon environments",
            Tags = ["dungeon", "fantasy", "classic", "rpg"],
            Tiles =
            [
                new TileEntity
                {
                    Id = "stone-wall",
                    Name = "Stone Wall",
                    Glyph = "█",
                    BackgroundColor = "#404040",
                    ForegroundColor = "#808080",
                    Comment = "Solid stone wall",
                    Description = "Impassable stone wall block",
                    Tags = ["wall", "stone", "solid", "impassable"]
                },
                new TileEntity
                {
                    Id = "stone-floor",
                    Name = "Stone Floor",
                    Glyph = ".",
                    BackgroundColor = "#202020",
                    ForegroundColor = "#606060",
                    Comment = "Stone floor tile",
                    Description = "Walkable stone floor",
                    Tags = ["floor", "stone", "walkable"]
                },
                new TileEntity
                {
                    Id = "door",
                    Name = "Wooden Door",
                    Glyph = "+",
                    BackgroundColor = "#8B4513",
                    ForegroundColor = "#DEB887",
                    Comment = "Wooden door",
                    Description = "A wooden door that can be opened or closed",
                    Tags = ["door", "wood", "interactive", "passable"]
                }
            ]
        };

        var json = JsonUtils.Serialize(originalTileSet);
        var deserializedTileSet = JsonUtils.Deserialize<TileSetEntity>(json);

        Assert.That(deserializedTileSet.Id, Is.EqualTo(originalTileSet.Id));
        Assert.That(deserializedTileSet.Name, Is.EqualTo(originalTileSet.Name));
        Assert.That(deserializedTileSet.Comment, Is.EqualTo(originalTileSet.Comment));
        Assert.That(deserializedTileSet.Description, Is.EqualTo(originalTileSet.Description));
        Assert.That(deserializedTileSet.Tags, Has.Count.EqualTo(originalTileSet.Tags.Count));
        Assert.That(deserializedTileSet.Tiles, Has.Count.EqualTo(3));

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
            Assert.That(deserializedTile.Description, Is.EqualTo(originalTile.Description));
            Assert.That(deserializedTile.Tags, Has.Count.EqualTo(originalTile.Tags.Count));
        }
    }

    [Test]
    public void JsonPropertyName_CommentField_ShouldUseHashSymbol()
    {
        var tileSet = new TileSetEntity
        {
            Id = "comment-test",
            Comment = "This is a tileset comment"
        };

        var json = JsonUtils.Serialize(tileSet);

        Assert.That(json, Contains.Substring("\"#\": \"This is a tileset comment\""));
        Assert.That(json, Does.Not.Contain("\"comment\""));
    }

    [Test]
    public void Serialize_LargeTileSet_ShouldHandlePerformance()
    {
        var tileSet = new TileSetEntity
        {
            Id = "large-set",
            Name = "Large Tileset"
        };

        for (var i = 0; i < 100; i++)
        {
            tileSet.Tiles.Add(
                new TileEntity
                {
                    Id = $"tile-{i:D3}",
                    Name = $"Tile {i}",
                    Glyph = ((char)('A' + i % 26)).ToString(),
                    Tags = ["auto-generated", $"batch-{i / 10}"]
                }
            );
        }

        var json = JsonUtils.Serialize(tileSet);
        var deserializedTileSet = JsonUtils.Deserialize<TileSetEntity>(json);

        Assert.That(deserializedTileSet.Tiles, Has.Count.EqualTo(100));
        Assert.That(deserializedTileSet.Tiles.First().Id, Is.EqualTo("tile-000"));
        Assert.That(deserializedTileSet.Tiles.Last().Id, Is.EqualTo("tile-099"));
    }
}
