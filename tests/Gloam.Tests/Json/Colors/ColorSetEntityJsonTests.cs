using Gloam.Core.Json;
using Gloam.Data.Context;
using Gloam.Data.Entities.Colors;

namespace Gloam.Tests.Json.Colors;

[TestFixture]
public class ColorSetEntityJsonTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        JsonUtils.RegisterJsonContext(new GloamDataJsonContext());
    }

    [Test]
    public void Serialize_EmptyColorSet_ShouldProduceValidJson()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "colorset-001",
            Name = "Empty ColorSet",
            Comment = "A colorset with no colors",
            Description = "Used for testing empty colorsets",
            Tags = ["test", "empty"]
        };

        var json = JsonUtils.Serialize(colorSet);

        Assert.That(json, Is.Not.Null.And.Not.Empty);
        Assert.That(json, Contains.Substring("colorset-001"));
        Assert.That(json, Contains.Substring("Empty ColorSet"));
        Assert.That(json, Contains.Substring("\"colors\": {}"));
        Assert.That(json, Contains.Substring("test"));
    }

    [Test]
    public void Serialize_ColorSetWithColors_ShouldIncludeAllColors()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "basic-colors",
            Name = "Basic Colors",
            Colors = new Dictionary<string, string>
            {
                ["white"] = "#FFFFFF",
                ["black"] = "#000000",
                ["red"] = "#FF0000",
                ["green"] = "#00FF00",
                ["blue"] = "#0000FF"
            }
        };

        var json = JsonUtils.Serialize(colorSet);

        Assert.That(json, Contains.Substring("basic-colors"));
        Assert.That(json, Contains.Substring("Basic Colors"));
        Assert.That(json, Contains.Substring("white"));
        Assert.That(json, Contains.Substring("#FFFFFF"));
        Assert.That(json, Contains.Substring("black"));
        Assert.That(json, Contains.Substring("#000000"));
        Assert.That(json, Contains.Substring("red"));
        Assert.That(json, Contains.Substring("#FF0000"));
    }

    [Test]
    public void Deserialize_ShouldRecreateCompleteObject()
    {
        var originalColorSet = new ColorSetEntity
        {
            Id = "web-colors",
            Name = "Web Colors",
            Comment = "Web safe colors",
            Description = "A collection of web-safe colors",
            Tags = ["web", "safe"],
            Colors = new Dictionary<string, string>
            {
                ["aliceBlue"] = "#F0F8FF",
                ["antiqueWhite"] = "#FAEBD7",
                ["aqua"] = "#00FFFF",
                ["crimson"] = "#DC143C"
            }
        };

        var json = JsonUtils.Serialize(originalColorSet);
        var deserializedColorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(deserializedColorSet, Is.Not.Null);
        Assert.That(deserializedColorSet.Id, Is.EqualTo(originalColorSet.Id));
        Assert.That(deserializedColorSet.Name, Is.EqualTo(originalColorSet.Name));
        Assert.That(deserializedColorSet.Comment, Is.EqualTo(originalColorSet.Comment));
        Assert.That(deserializedColorSet.Description, Is.EqualTo(originalColorSet.Description));
        Assert.That(deserializedColorSet.Tags, Is.EqualTo(originalColorSet.Tags));
        Assert.That(deserializedColorSet.Colors, Has.Count.EqualTo(4));

        Assert.That(deserializedColorSet.Colors["aliceBlue"], Is.EqualTo("#F0F8FF"));
        Assert.That(deserializedColorSet.Colors["antiqueWhite"], Is.EqualTo("#FAEBD7"));
        Assert.That(deserializedColorSet.Colors["aqua"], Is.EqualTo("#00FFFF"));
        Assert.That(deserializedColorSet.Colors["crimson"], Is.EqualTo("#DC143C"));
    }

    [Test]
    public void Serialize_WithNullProperties_ShouldHandleGracefully()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "minimal-colors",
            Name = "Minimal Colors"
        };

        var json = JsonUtils.Serialize(colorSet);
        var deserializedColorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(deserializedColorSet.Id, Is.EqualTo("minimal-colors"));
        Assert.That(deserializedColorSet.Name, Is.EqualTo("Minimal Colors"));
        Assert.That(deserializedColorSet.Comment, Is.Null);
        Assert.That(deserializedColorSet.Description, Is.Null);
        Assert.That(deserializedColorSet.Tags, Is.Not.Null.And.Empty);
        Assert.That(deserializedColorSet.Colors, Is.Not.Null.And.Empty);
    }

    [Test]
    public void Deserialize_WithMissingColors_ShouldInitializeEmptyDictionary()
    {
        var json = """
                   {
                       "id": "no-colors-set",
                       "name": "No Colors Set"
                   }
                   """;

        var colorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(colorSet.Id, Is.EqualTo("no-colors-set"));
        Assert.That(colorSet.Name, Is.EqualTo("No Colors Set"));
        Assert.That(colorSet.Colors, Is.Not.Null.And.Empty);
    }

    [Test]
    public void RoundTrip_ComplexColorSet_ShouldPreserveAllData()
    {
        var originalColorSet = new ColorSetEntity
        {
            Id = "material-design",
            Name = "Material Design Colors",
            Comment = "Google Material Design color palette",
            Description = "A comprehensive set of colors from Google's Material Design guidelines",
            Tags = ["material", "design", "google", "ui"],
            Colors = new Dictionary<string, string>
            {
                ["red50"] = "#FFEBEE",
                ["red100"] = "#FFCDD2",
                ["red500"] = "#F44336",
                ["red900"] = "#B71C1C",
                ["blue50"] = "#E3F2FD",
                ["blue100"] = "#BBDEFB",
                ["blue500"] = "#2196F3",
                ["blue900"] = "#0D47A1",
                ["green50"] = "#E8F5E8",
                ["green500"] = "#4CAF50",
                ["amber500"] = "#FFC107",
                ["deepOrange500"] = "#FF5722"
            }
        };

        var json = JsonUtils.Serialize(originalColorSet);
        var deserializedColorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(deserializedColorSet.Id, Is.EqualTo(originalColorSet.Id));
        Assert.That(deserializedColorSet.Name, Is.EqualTo(originalColorSet.Name));
        Assert.That(deserializedColorSet.Comment, Is.EqualTo(originalColorSet.Comment));
        Assert.That(deserializedColorSet.Description, Is.EqualTo(originalColorSet.Description));
        Assert.That(deserializedColorSet.Tags, Has.Count.EqualTo(originalColorSet.Tags.Count));
        Assert.That(deserializedColorSet.Colors, Has.Count.EqualTo(originalColorSet.Colors.Count));

        foreach (var originalTag in originalColorSet.Tags)
        {
            Assert.That(deserializedColorSet.Tags, Contains.Item(originalTag));
        }

        foreach (var originalColor in originalColorSet.Colors)
        {
            Assert.That(deserializedColorSet.Colors.ContainsKey(originalColor.Key), Is.True);
            Assert.That(deserializedColorSet.Colors[originalColor.Key], Is.EqualTo(originalColor.Value));
        }
    }

    [Test]
    public void JsonPropertyName_CommentField_ShouldUseHashSymbol()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "comment-test",
            Comment = "This is a colorset comment"
        };

        var json = JsonUtils.Serialize(colorSet);

        Assert.That(json, Contains.Substring("\"#\": \"This is a colorset comment\""));
        Assert.That(json, Does.Not.Contain("\"comment\""));
    }

    [Test]
    public void Serialize_ColorNamesWithSpecialCharacters_ShouldHandleCorrectly()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "special-chars",
            Name = "Special Character Colors",
            Colors = new Dictionary<string, string>
            {
                ["color-with-dash"] = "#FF0000",
                ["color_with_underscore"] = "#00FF00",
                ["color with space"] = "#0000FF",
                ["colorWithCamelCase"] = "#FFFF00",
                ["color123"] = "#FF00FF",
                ["αβγ"] = "#00FFFF"
            }
        };

        var json = JsonUtils.Serialize(colorSet);
        var deserializedColorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(deserializedColorSet.Colors, Has.Count.EqualTo(6));
        Assert.That(deserializedColorSet.Colors["color-with-dash"], Is.EqualTo("#FF0000"));
        Assert.That(deserializedColorSet.Colors["color_with_underscore"], Is.EqualTo("#00FF00"));
        Assert.That(deserializedColorSet.Colors["color with space"], Is.EqualTo("#0000FF"));
        Assert.That(deserializedColorSet.Colors["colorWithCamelCase"], Is.EqualTo("#FFFF00"));
        Assert.That(deserializedColorSet.Colors["color123"], Is.EqualTo("#FF00FF"));
        Assert.That(deserializedColorSet.Colors["αβγ"], Is.EqualTo("#00FFFF"));
    }

    [Test]
    public void Serialize_LargeColorSet_ShouldHandlePerformance()
    {
        var colorSet = new ColorSetEntity
        {
            Id = "large-palette",
            Name = "Large Color Palette"
        };

        for (int r = 0; r < 16; r++)
        {
            for (int g = 0; g < 16; g++)
            {
                for (int b = 0; b < 4; b++)
                {
                    var hex = $"#{r:X}{r:X}{g:X}{g:X}{b:X}{b:X}";
                    var name = $"color_{r:X}{g:X}{b:X}";
                    colorSet.Colors.Add(name, hex);
                }
            }
        }

        var json = JsonUtils.Serialize(colorSet);
        var deserializedColorSet = JsonUtils.Deserialize<ColorSetEntity>(json);

        Assert.That(deserializedColorSet.Colors, Has.Count.EqualTo(1024));
        Assert.That(deserializedColorSet.Colors["color_000"], Is.EqualTo("#000000"));
        Assert.That(deserializedColorSet.Colors["color_FF3"], Is.EqualTo("#FFFF33"));
    }
}
