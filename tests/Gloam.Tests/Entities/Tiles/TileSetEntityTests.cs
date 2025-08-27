using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Entities.Tiles;

[TestFixture]
public class TileSetEntityTests
{
    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        var tileSet = new TileSetEntity();

        Assert.That(tileSet.Tiles, Is.Not.Null);
        Assert.That(tileSet.Tiles, Is.Empty);
        Assert.That(tileSet.Id, Is.Null);
        Assert.That(tileSet.Name, Is.Null);
        Assert.That(tileSet.Comment, Is.Null);
        Assert.That(tileSet.Description, Is.Null);
        Assert.That(tileSet.Tags, Is.Not.Null);
        Assert.That(tileSet.Tags, Is.Empty);
    }

    [Test]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        var tileSet = new TileSetEntity
        {
            Id = "tileset-001",
            Name = "Dungeon Tiles",
            Comment = "Basic dungeon tileset",
            Description = "A collection of tiles for dungeon environments",
            Tags = ["dungeon", "fantasy", "basic"]
        };

        Assert.That(tileSet.Id, Is.EqualTo("tileset-001"));
        Assert.That(tileSet.Name, Is.EqualTo("Dungeon Tiles"));
        Assert.That(tileSet.Comment, Is.EqualTo("Basic dungeon tileset"));
        Assert.That(tileSet.Description, Is.EqualTo("A collection of tiles for dungeon environments"));
        Assert.That(tileSet.Tags, Has.Count.EqualTo(3));
        Assert.That(tileSet.Tags, Contains.Item("dungeon"));
        Assert.That(tileSet.Tags, Contains.Item("fantasy"));
        Assert.That(tileSet.Tags, Contains.Item("basic"));
    }

    [Test]
    public void Tiles_ShouldAllowAddingTiles()
    {
        var tileSet = new TileSetEntity();
        var tile1 = new TileEntity { Id = "tile-1", Glyph = "#", Name = "Wall" };
        var tile2 = new TileEntity { Id = "tile-2", Glyph = ".", Name = "Floor" };

        tileSet.Tiles.Add(tile1);
        tileSet.Tiles.Add(tile2);

        Assert.That(tileSet.Tiles, Has.Count.EqualTo(2));
        Assert.That(tileSet.Tiles, Contains.Item(tile1));
        Assert.That(tileSet.Tiles, Contains.Item(tile2));
    }

    [Test]
    public void Tiles_ShouldAllowRemovingTiles()
    {
        var tileSet = new TileSetEntity();
        var tile1 = new TileEntity { Id = "tile-1", Glyph = "#", Name = "Wall" };
        var tile2 = new TileEntity { Id = "tile-2", Glyph = ".", Name = "Floor" };

        tileSet.Tiles.Add(tile1);
        tileSet.Tiles.Add(tile2);

        Assert.That(tileSet.Tiles, Has.Count.EqualTo(2));

        tileSet.Tiles.Remove(tile1);

        Assert.That(tileSet.Tiles, Has.Count.EqualTo(1));
        Assert.That(tileSet.Tiles, Does.Not.Contain(tile1));
        Assert.That(tileSet.Tiles, Contains.Item(tile2));
    }

    [Test]
    public void Tiles_ShouldAllowClearingAllTiles()
    {
        var tileSet = new TileSetEntity();
        var tile1 = new TileEntity { Id = "tile-1", Glyph = "#", Name = "Wall" };
        var tile2 = new TileEntity { Id = "tile-2", Glyph = ".", Name = "Floor" };

        tileSet.Tiles.Add(tile1);
        tileSet.Tiles.Add(tile2);

        Assert.That(tileSet.Tiles, Has.Count.EqualTo(2));

        tileSet.Tiles.Clear();

        Assert.That(tileSet.Tiles, Is.Empty);
    }

    [Test]
    public void Tiles_ShouldSupportComplexTileConfiguration()
    {
        var tileSet = new TileSetEntity
        {
            Id = "complex-set",
            Name = "Complex Tileset"
        };

        var wallTile = new TileEntity
        {
            Id = "wall-tile",
            Name = "Stone Wall",
            Glyph = "#",
            BackgroundColor = "#404040",
            ForegroundColor = "#808080",
            Tags = ["wall", "solid", "stone"]
        };

        var floorTile = new TileEntity
        {
            Id = "floor-tile",
            Name = "Stone Floor",
            Glyph = ".",
            BackgroundColor = "#202020",
            ForegroundColor = "#606060",
            Tags = ["floor", "walkable", "stone"]
        };

        tileSet.Tiles.Add(wallTile);
        tileSet.Tiles.Add(floorTile);

        Assert.That(tileSet.Tiles, Has.Count.EqualTo(2));

        var retrievedWall = tileSet.Tiles.First(t => t.Id == "wall-tile");
        Assert.That(retrievedWall.Name, Is.EqualTo("Stone Wall"));
        Assert.That(retrievedWall.Glyph, Is.EqualTo("#"));
        Assert.That(retrievedWall.BackgroundColor, Is.EqualTo("#404040"));
        Assert.That(retrievedWall.Tags, Contains.Item("wall"));

        var retrievedFloor = tileSet.Tiles.First(t => t.Id == "floor-tile");
        Assert.That(retrievedFloor.Name, Is.EqualTo("Stone Floor"));
        Assert.That(retrievedFloor.Glyph, Is.EqualTo("."));
        Assert.That(retrievedFloor.Tags, Contains.Item("walkable"));
    }

    [Test]
    public void Tags_ShouldBeModifiable()
    {
        var tileSet = new TileSetEntity();

        tileSet.Tags.Add("indoor");
        tileSet.Tags.Add("medieval");

        Assert.That(tileSet.Tags, Has.Count.EqualTo(2));
        Assert.That(tileSet.Tags, Contains.Item("indoor"));
        Assert.That(tileSet.Tags, Contains.Item("medieval"));

        tileSet.Tags.Remove("indoor");

        Assert.That(tileSet.Tags, Has.Count.EqualTo(1));
        Assert.That(tileSet.Tags, Does.Not.Contain("indoor"));
        Assert.That(tileSet.Tags, Contains.Item("medieval"));
    }
}
