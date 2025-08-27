using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Entities.Tiles;

[TestFixture]
public class TileEntityTests
{
    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        var tile = new TileEntity();

        Assert.That(tile.Glyph, Is.Null);
        Assert.That(tile.BackgroundColor, Is.Null);
        Assert.That(tile.ForegroundColor, Is.Null);
        Assert.That(tile.Id, Is.Null);
        Assert.That(tile.Name, Is.Null);
        Assert.That(tile.Comment, Is.Null);
        Assert.That(tile.Description, Is.Null);
        Assert.That(tile.Tags, Is.Not.Null);
        Assert.That(tile.Tags, Is.Empty);
    }

    [Test]
    public void Properties_ShouldSetAndGetCorrectly()
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

        Assert.That(tile.Id, Is.EqualTo("tile-001"));
        Assert.That(tile.Name, Is.EqualTo("Wall Tile"));
        Assert.That(tile.Glyph, Is.EqualTo("#"));
        Assert.That(tile.BackgroundColor, Is.EqualTo("#333333"));
        Assert.That(tile.ForegroundColor, Is.EqualTo("#FFFFFF"));
        Assert.That(tile.Comment, Is.EqualTo("A solid wall tile"));
        Assert.That(tile.Description, Is.EqualTo("Used for walls and barriers"));
        Assert.That(tile.Tags, Has.Count.EqualTo(3));
        Assert.That(tile.Tags, Contains.Item("wall"));
        Assert.That(tile.Tags, Contains.Item("solid"));
        Assert.That(tile.Tags, Contains.Item("barrier"));
    }

    [Test]
    public void Glyph_ShouldAcceptSingleCharacter()
    {
        var tile = new TileEntity { Glyph = "@" };

        Assert.That(tile.Glyph, Is.EqualTo("@"));
    }

    [Test]
    public void Colors_ShouldAcceptHexValues()
    {
        var tile = new TileEntity
        {
            BackgroundColor = "#FF0000",
            ForegroundColor = "#00FF00"
        };

        Assert.That(tile.BackgroundColor, Is.EqualTo("#FF0000"));
        Assert.That(tile.ForegroundColor, Is.EqualTo("#00FF00"));
    }

    [Test]
    public void Tags_ShouldBeModifiable()
    {
        var tile = new TileEntity();

        tile.Tags.Add("floor");
        tile.Tags.Add("walkable");

        Assert.That(tile.Tags, Has.Count.EqualTo(2));
        Assert.That(tile.Tags, Contains.Item("floor"));
        Assert.That(tile.Tags, Contains.Item("walkable"));

        tile.Tags.Remove("floor");

        Assert.That(tile.Tags, Has.Count.EqualTo(1));
        Assert.That(tile.Tags, Does.Not.Contain("floor"));
        Assert.That(tile.Tags, Contains.Item("walkable"));
    }
}
