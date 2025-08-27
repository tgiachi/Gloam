using Gloam.Core.Json;
using Gloam.Data.Context;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Json.Tiles;

[TestFixture]
public class TileEntityJsonTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        JsonUtils.RegisterJsonContext(new GloamDataJsonContext());
    }

    [Test]
    public void Serialize_ShouldProduceValidJson()
    {
        var tile = new TileEntity
        {
            Id = "tile-001",
            Name = "Wall Tile",
            Glyph = "#",
            BackgroundColor = "#333333",
            ForegroundColor = "#FFFFFF",
            Comment = "A solid wall tile",
            Description = "Used for walls and barriers",
            Tags = ["wall", "solid", "barrier"]
        };

        var json = JsonUtils.Serialize(tile);

        Assert.That(json, Is.Not.Null.And.Not.Empty);
        Assert.That(json, Contains.Substring("tile-001"));
        Assert.That(json, Contains.Substring("Wall Tile"));
        Assert.That(json, Contains.Substring("#333333"));
        Assert.That(json, Contains.Substring("wall"));
    }

    [Test]
    public void Deserialize_ShouldRecreateObject()
    {
        var originalTile = new TileEntity
        {
            Id = "tile-002",
            Name = "Floor Tile",
            Glyph = ".",
            BackgroundColor = "#202020",
            ForegroundColor = "#808080",
            Comment = "A walkable floor tile",
            Description = "Used for floors and walkable areas",
            Tags = ["floor", "walkable"]
        };

        var json = JsonUtils.Serialize(originalTile);
        var deserializedTile = JsonUtils.Deserialize<TileEntity>(json);

        Assert.That(deserializedTile, Is.Not.Null);
        Assert.That(deserializedTile.Id, Is.EqualTo(originalTile.Id));
        Assert.That(deserializedTile.Name, Is.EqualTo(originalTile.Name));
        Assert.That(deserializedTile.Glyph, Is.EqualTo(originalTile.Glyph));
        Assert.That(deserializedTile.BackgroundColor, Is.EqualTo(originalTile.BackgroundColor));
        Assert.That(deserializedTile.ForegroundColor, Is.EqualTo(originalTile.ForegroundColor));
        Assert.That(deserializedTile.Comment, Is.EqualTo(originalTile.Comment));
        Assert.That(deserializedTile.Description, Is.EqualTo(originalTile.Description));
        Assert.That(deserializedTile.Tags, Is.EqualTo(originalTile.Tags));
    }

    [Test]
    public void Serialize_WithNullProperties_ShouldHandleGracefully()
    {
        var tile = new TileEntity
        {
            Id = "tile-003",
            Name = "Simple Tile",
            Glyph = "@"
        };

        var json = JsonUtils.Serialize(tile);
        var deserializedTile = JsonUtils.Deserialize<TileEntity>(json);

        Assert.That(deserializedTile.Id, Is.EqualTo("tile-003"));
        Assert.That(deserializedTile.Name, Is.EqualTo("Simple Tile"));
        Assert.That(deserializedTile.Glyph, Is.EqualTo("@"));
        Assert.That(deserializedTile.BackgroundColor, Is.Null);
        Assert.That(deserializedTile.ForegroundColor, Is.Null);
        Assert.That(deserializedTile.Comment, Is.Null);
        Assert.That(deserializedTile.Description, Is.Null);
        Assert.That(deserializedTile.Tags, Is.Not.Null.And.Empty);
    }

    [Test]
    public void Serialize_WithEmptyTags_ShouldSerializeAsEmptyArray()
    {
        var tile = new TileEntity
        {
            Id = "tile-004",
            Name = "No Tags Tile",
            Tags = []
        };

        var json = JsonUtils.Serialize(tile);

        Assert.That(json, Contains.Substring("\"tags\": []"));
    }

    [Test]
    public void Deserialize_WithMissingProperties_ShouldUseDefaults()
    {
        var json = """
                   {
                       "id": "minimal-tile",
                       "name": "Minimal Tile"
                   }
                   """;

        var tile = JsonUtils.Deserialize<TileEntity>(json);

        Assert.That(tile.Id, Is.EqualTo("minimal-tile"));
        Assert.That(tile.Name, Is.EqualTo("Minimal Tile"));
        Assert.That(tile.Glyph, Is.Null);
        Assert.That(tile.BackgroundColor, Is.Null);
        Assert.That(tile.ForegroundColor, Is.Null);
        Assert.That(tile.Tags, Is.Not.Null.And.Empty);
    }

    [Test]
    public void JsonPropertyName_CommentField_ShouldUseHashSymbol()
    {
        var tile = new TileEntity
        {
            Id = "comment-test",
            Comment = "This is a comment"
        };

        var json = JsonUtils.Serialize(tile);

        Assert.That(json, Contains.Substring("\"#\": \"This is a comment\""));
        Assert.That(json, Does.Not.Contain("\"comment\""));
    }

    [Test]
    public void RoundTrip_ComplexTile_ShouldPreserveAllData()
    {
        var originalTile = new TileEntity
        {
            Id = "complex-tile-001",
            Name = "Complex Wall Tile",
            Glyph = "█",
            BackgroundColor = "#2D2D30",
            ForegroundColor = "#FFFFFF",
            Comment = "A complex wall tile with unicode glyph",
            Description = "This tile represents a solid wall using a unicode block character",
            Tags = ["wall", "solid", "unicode", "block", "impassable"]
        };

        var json = JsonUtils.Serialize(originalTile);
        var deserializedTile = JsonUtils.Deserialize<TileEntity>(json);

        Assert.That(deserializedTile.Id, Is.EqualTo(originalTile.Id));
        Assert.That(deserializedTile.Name, Is.EqualTo(originalTile.Name));
        Assert.That(deserializedTile.Glyph, Is.EqualTo(originalTile.Glyph));
        Assert.That(deserializedTile.BackgroundColor, Is.EqualTo(originalTile.BackgroundColor));
        Assert.That(deserializedTile.ForegroundColor, Is.EqualTo(originalTile.ForegroundColor));
        Assert.That(deserializedTile.Comment, Is.EqualTo(originalTile.Comment));
        Assert.That(deserializedTile.Description, Is.EqualTo(originalTile.Description));
        Assert.That(deserializedTile.Tags, Has.Count.EqualTo(originalTile.Tags.Count));

        foreach (var tag in originalTile.Tags)
        {
            Assert.That(deserializedTile.Tags, Contains.Item(tag));
        }
    }
}
