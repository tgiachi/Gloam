using Gloam.Data.Entities.Colors;

namespace Gloam.Tests.Entities.Colors;

[TestFixture]
public class ColorSetEntityTests
{
    [Test]
    public void Constructor_ShouldInitializeProperties()
    {
        var colorSet = new ColorSetEntity();

        Assert.That(colorSet.Colors, Is.Not.Null);
        Assert.That(colorSet.Colors, Is.Empty);
        Assert.That(colorSet.Id, Is.Null);
        Assert.That(colorSet.Name, Is.Null);
        Assert.That(colorSet.Comment, Is.Null);
        Assert.That(colorSet.Description, Is.Null);
        Assert.That(colorSet.Tags, Is.Not.Null);
        Assert.That(colorSet.Tags, Is.Empty);
    }

    [Test]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "colorset-001",
            Name = "Basic Colors",
            Comment = "Basic color palette",
            Description = "A collection of basic colors for UI elements",
            Tags = ["basic", "ui", "palette"]
        };

        Assert.That(colorSet.Id, Is.EqualTo("colorset-001"));
        Assert.That(colorSet.Name, Is.EqualTo("Basic Colors"));
        Assert.That(colorSet.Comment, Is.EqualTo("Basic color palette"));
        Assert.That(colorSet.Description, Is.EqualTo("A collection of basic colors for UI elements"));
        Assert.That(colorSet.Tags, Has.Count.EqualTo(3));
        Assert.That(colorSet.Tags, Contains.Item("basic"));
        Assert.That(colorSet.Tags, Contains.Item("ui"));
        Assert.That(colorSet.Tags, Contains.Item("palette"));
    }

    [Test]
    public void Colors_ShouldAllowAddingColors()
    {
        var colorSet = new ColorSetEntity();

        colorSet.Colors.Add("white", "#FFFFFF");
        colorSet.Colors.Add("black", "#000000");
        colorSet.Colors.Add("red", "#FF0000");

        Assert.That(colorSet.Colors, Has.Count.EqualTo(3));
        Assert.That(colorSet.Colors["white"], Is.EqualTo("#FFFFFF"));
        Assert.That(colorSet.Colors["black"], Is.EqualTo("#000000"));
        Assert.That(colorSet.Colors["red"], Is.EqualTo("#FF0000"));
    }

    [Test]
    public void Colors_ShouldAllowRemovingColors()
    {
        var colorSet = new ColorSetEntity();
        colorSet.Colors.Add("white", "#FFFFFF");
        colorSet.Colors.Add("black", "#000000");
        colorSet.Colors.Add("red", "#FF0000");

        Assert.That(colorSet.Colors, Has.Count.EqualTo(3));

        colorSet.Colors.Remove("black");

        Assert.That(colorSet.Colors, Has.Count.EqualTo(2));
        Assert.That(colorSet.Colors.ContainsKey("black"), Is.False);
        Assert.That(colorSet.Colors.ContainsKey("white"), Is.True);
        Assert.That(colorSet.Colors.ContainsKey("red"), Is.True);
    }

    [Test]
    public void Colors_ShouldAllowClearingAllColors()
    {
        var colorSet = new ColorSetEntity();
        colorSet.Colors.Add("white", "#FFFFFF");
        colorSet.Colors.Add("black", "#000000");

        Assert.That(colorSet.Colors, Has.Count.EqualTo(2));

        colorSet.Colors.Clear();

        Assert.That(colorSet.Colors, Is.Empty);
    }

    [Test]
    public void Colors_ShouldSupportComplexColorConfiguration()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "web-colors",
            Name = "Web Safe Colors"
        };

        var webColors = new Dictionary<string, string>
        {
            ["aliceBlue"] = "#F0F8FF",
            ["antiqueWhite"] = "#FAEBD7",
            ["aqua"] = "#00FFFF",
            ["aquamarine"] = "#7FFFD4",
            ["azure"] = "#F0FFFF",
            ["beige"] = "#F5F5DC",
            ["bisque"] = "#FFE4C4",
            ["blanchedAlmond"] = "#FFEBCD"
        };

        foreach (var color in webColors)
        {
            colorSet.Colors.Add(color.Key, color.Value);
        }

        Assert.That(colorSet.Colors, Has.Count.EqualTo(8));
        Assert.That(colorSet.Colors["aliceBlue"], Is.EqualTo("#F0F8FF"));
        Assert.That(colorSet.Colors["antiqueWhite"], Is.EqualTo("#FAEBD7"));
        Assert.That(colorSet.Colors["blanchedAlmond"], Is.EqualTo("#FFEBCD"));
    }

    [Test]
    public void Colors_ShouldHandleColorUpdates()
    {
        var colorSet = new ColorSetEntity();
        colorSet.Colors.Add("primary", "#FF0000");

        Assert.That(colorSet.Colors["primary"], Is.EqualTo("#FF0000"));

        colorSet.Colors["primary"] = "#0000FF";

        Assert.That(colorSet.Colors["primary"], Is.EqualTo("#0000FF"));
        Assert.That(colorSet.Colors, Has.Count.EqualTo(1));
    }

    [Test]
    public void Colors_ShouldSupportCaseVariations()
    {
        var colorSet = new ColorSetEntity();

        colorSet.Colors.Add("Red", "#FF0000");
        colorSet.Colors.Add("GREEN", "#00FF00");
        colorSet.Colors.Add("blue", "#0000FF");
        colorSet.Colors.Add("White", "#FFFFFF");

        Assert.That(colorSet.Colors, Has.Count.EqualTo(4));
        Assert.That(colorSet.Colors["Red"], Is.EqualTo("#FF0000"));
        Assert.That(colorSet.Colors["GREEN"], Is.EqualTo("#00FF00"));
        Assert.That(colorSet.Colors["blue"], Is.EqualTo("#0000FF"));
        Assert.That(colorSet.Colors["White"], Is.EqualTo("#FFFFFF"));
    }

    [Test]
    public void Tags_ShouldBeModifiable()
    {
        var colorSet = new ColorSetEntity();

        colorSet.Tags.Add("ui");
        colorSet.Tags.Add("theme");

        Assert.That(colorSet.Tags, Has.Count.EqualTo(2));
        Assert.That(colorSet.Tags, Contains.Item("ui"));
        Assert.That(colorSet.Tags, Contains.Item("theme"));

        colorSet.Tags.Remove("ui");

        Assert.That(colorSet.Tags, Has.Count.EqualTo(1));
        Assert.That(colorSet.Tags, Does.Not.Contain("ui"));
        Assert.That(colorSet.Tags, Contains.Item("theme"));
    }

    [Test]
    public void Colors_ShouldHandleHexColorFormats()
    {
        var colorSet = new ColorSetEntity();

        colorSet.Colors.Add("shortHex", "#FFF");
        colorSet.Colors.Add("longHex", "#FFFFFF");
        colorSet.Colors.Add("withAlpha", "#FFFFFF80");
        colorSet.Colors.Add("lowercase", "#ffffff");
        colorSet.Colors.Add("uppercase", "#FFFFFF");

        Assert.That(colorSet.Colors["shortHex"], Is.EqualTo("#FFF"));
        Assert.That(colorSet.Colors["longHex"], Is.EqualTo("#FFFFFF"));
        Assert.That(colorSet.Colors["withAlpha"], Is.EqualTo("#FFFFFF80"));
        Assert.That(colorSet.Colors["lowercase"], Is.EqualTo("#ffffff"));
        Assert.That(colorSet.Colors["uppercase"], Is.EqualTo("#FFFFFF"));
    }
}
