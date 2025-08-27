using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gloam.Core.Json;
using Gloam.Data.Context;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;

namespace Gloam.Tests.Json;

/// <summary>
///     Tests for JsonUtils functionality.
/// </summary>
public class JsonUtilsTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Register the data context for entity serialization
        var dataContext = new GloamDataJsonContext();
        JsonUtils.RegisterJsonContext(dataContext);
    }

    #region Schema File Name Tests

    [Test]
    public void GetSchemaFileName_BaseGloamEntity_ReturnsCorrectName()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(BaseGloamEntity));
        Assert.That(fileName, Is.EqualTo("base_gloam.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_TileEntity_ReturnsCorrectName()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(TileEntity));
        Assert.That(fileName, Is.EqualTo("tile.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_TileSetEntity_ReturnsCorrectName()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(TileSetEntity));
        Assert.That(fileName, Is.EqualTo("tile_set.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_ColorSetEntity_ReturnsCorrectName()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(ColorSetEntity));
        Assert.That(fileName, Is.EqualTo("color_set.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.GetSchemaFileName(null!));
    }

    [Test]
    public void GetSchemaFileName_TypeWithoutEntitySuffix_ReturnsCorrectName()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(string));
        Assert.That(fileName, Is.EqualTo("string.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_PascalCaseType_ConvertsToSnakeCase()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(MyComplexTypeEntity));
        Assert.That(fileName, Is.EqualTo("my_complex_type.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_EntitySuffix_ShouldBeRemoved()
    {
        var fileName1 = JsonUtils.GetSchemaFileName(typeof(TileEntity));
        var fileName2 = JsonUtils.GetSchemaFileName(typeof(ColorSetEntity));

        Assert.That(fileName1, Is.EqualTo("tile.schema.json"));
        Assert.That(fileName2, Is.EqualTo("color_set.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_ComplexPascalCase_ShouldConvertCorrectly()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(MyVeryComplexTypeName));
        Assert.That(fileName, Is.EqualTo("my_very_complex_type_name.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_SingleWord_ShouldWork()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(SimpleEntity));
        Assert.That(fileName, Is.EqualTo("simple.schema.json"));
    }

    [Test]
    public void GetSchemaFileName_AlreadyLowerCase_ShouldWork()
    {
        var fileName = JsonUtils.GetSchemaFileName(typeof(TestType));
        Assert.That(fileName, Is.EqualTo("test_type.schema.json"));
    }

    #endregion

    #region Basic Functionality Tests

    [Test]
    public void Serialize_NullObject_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.Serialize<object>(null!));
    }

    [Test]
    public void Deserialize_NullJson_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.Deserialize<TileEntity>(null!));
    }

    [Test]
    public void Deserialize_EmptyString_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => JsonUtils.Deserialize<TileEntity>(""));
    }

    [Test]
    public void SerializeToFile_NullObject_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.SerializeToFile<TileEntity>(null!, "test.json"));
    }

    [Test]
    public void SerializeToFile_NullPath_ShouldThrowArgumentNullException()
    {
        var testObject = new TileEntity
        {
            Id = "test",
            Name = "Test",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };

        Assert.Throws<ArgumentNullException>(() => JsonUtils.SerializeToFile(testObject, null!));
    }

    [Test]
    public void DeserializeFromFile_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        Assert.Throws<FileNotFoundException>(() => JsonUtils.DeserializeFromFile<TileEntity>(nonExistentPath));
    }

    [Test]
    public async Task DeserializeFromFileAsync_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await JsonUtils.DeserializeFromFileAsync<TileEntity>(nonExistentPath)
        );
    }

    #endregion

    #region Serialization and Deserialization Tests

    [Test]
    public void Serialize_ValidObject_ReturnsJsonString()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };

        var json = JsonUtils.Serialize(testObject);

        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("test-tile"));
        Assert.That(json, Does.Contain("Test Tile"));
    }

    [Test]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        var json = """
                   {
                       "id": "test-tile",
                       "name": "Test Tile",
                       "glyph": "@",
                       "backgroundColor": "#000000",
                       "foregroundColor": "#FFFFFF"
                   }
                   """;

        var result = JsonUtils.Deserialize<TileEntity>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-tile"));
        Assert.That(result.Name, Is.EqualTo("Test Tile"));
    }

    [Test]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var invalidJson = "null";

        Assert.Throws<JsonException>(() => JsonUtils.Deserialize<TileEntity>(invalidJson));
    }

    [Test]
    public void Deserialize_ValidJsonWithNullResult_ThrowsJsonException()
    {
        var json = "null";

        Assert.Throws<JsonException>(() => JsonUtils.Deserialize<string>(json));
    }

    [Test]
    public void Deserialize_WhitespaceString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => JsonUtils.Deserialize<TileEntity>("   "));
    }

    [Test]
    public void SerializeToFile_ValidObjectAndPath_CreatesFile()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            JsonUtils.SerializeToFile(testObject, tempFilePath);

            Assert.That(File.Exists(tempFilePath), Is.True);
            var content = File.ReadAllText(tempFilePath);
            Assert.That(content, Does.Contain("test-tile"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task SerializeToFileAsync_ValidObjectAndPath_CreatesFile()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            await JsonUtils.SerializeToFileAsync(testObject, tempFilePath);

            Assert.That(File.Exists(tempFilePath), Is.True);
            var content = File.ReadAllText(tempFilePath);
            Assert.That(content, Does.Contain("test-tile"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task SerializeToFileAsync_NullObject_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await JsonUtils.SerializeToFileAsync<TileEntity>(null!, "test.json")
        );
    }

    [Test]
    public async Task SerializeToFileAsync_NullPath_ThrowsArgumentNullException()
    {
        var testObject = new TileEntity
        {
            Id = "test",
            Name = "Test",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await JsonUtils.SerializeToFileAsync(testObject, null!)
        );
    }

    [Test]
    public void DeserializeFromFile_ValidFile_ReturnsObject()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            JsonUtils.SerializeToFile(testObject, tempFilePath);
            var result = JsonUtils.DeserializeFromFile<TileEntity>(tempFilePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("test-tile"));
            Assert.That(result.Name, Is.EqualTo("Test Tile"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task DeserializeFromFileAsync_ValidFile_ReturnsObject()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            await JsonUtils.SerializeToFileAsync(testObject, tempFilePath);
            var result = await JsonUtils.DeserializeFromFileAsync<TileEntity>(tempFilePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("test-tile"));
            Assert.That(result.Name, Is.EqualTo("Test Tile"));
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Test]
    public async Task DeserializeFromFileAsync_NullPath_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await JsonUtils.DeserializeFromFileAsync<TileEntity>(null!)
        );
    }

    #endregion

    #region Converter Management Tests

    [Test]
    public void AddJsonConverter_ValidConverter_AddsToList()
    {
        var initialConverters = JsonUtils.GetJsonConverters();
        var testConverter = new JsonStringEnumConverter();

        JsonUtils.AddJsonConverter(testConverter);
        var newConverters = JsonUtils.GetJsonConverters();

        Assert.That(newConverters.Count, Is.GreaterThanOrEqualTo(initialConverters.Count));
        Assert.That(newConverters.Any(c => c.GetType() == testConverter.GetType()), Is.True);
    }

    [Test]
    public void AddJsonConverter_NullConverter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.AddJsonConverter(null!));
    }

    #endregion

    #region Context Management Tests

    [Test]
    public void RegisterJsonContext_ValidContext_AddsToContexts()
    {
        // Use the existing context from Gloam.Data since it's already available
        var dataContext = new GloamDataJsonContext();

        // This should not throw - we're testing the null check and basic functionality
        Assert.DoesNotThrow(() => JsonUtils.RegisterJsonContext(dataContext));
    }

    [Test]
    public void RegisterJsonContext_NullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.RegisterJsonContext(null!));
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public void GetSchemaFileName_EmptyStringType_HandlesCorrectly()
    {
        // Test with a type that has an empty name scenario
        var fileName = JsonUtils.GetSchemaFileName(typeof(TestEmptyType));
        Assert.That(fileName, Is.EqualTo("test_empty_type.schema.json"));
    }

    [Test]
    public void ConvertToSnakeCase_WithEmptyString_ShouldReturnEmpty()
    {
        // Test the edge case where pascalCase is empty via reflection
        var jsonUtilsType = typeof(JsonUtils);
        var convertMethod = jsonUtilsType.GetMethod(
            "ConvertToSnakeCase",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        var result = convertMethod?.Invoke(null, new object[] { "" });
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void ConvertToSnakeCase_WithNullString_ShouldReturnNull()
    {
        // Test the edge case where pascalCase is null via reflection
        var jsonUtilsType = typeof(JsonUtils);
        var convertMethod = jsonUtilsType.GetMethod(
            "ConvertToSnakeCase",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        var result = convertMethod?.Invoke(null, new object[] { null! });
        Assert.That(result, Is.Null);
    }

    #endregion

    #region New API Tests

    [Test]
    public void DeserializeOrDefault_ValidJson_ReturnsObject()
    {
        var json = """
                   {
                       "id": "test-tile",
                       "name": "Test Tile",
                       "glyph": "@",
                       "backgroundColor": "#000000",
                       "foregroundColor": "#FFFFFF"
                   }
                   """;

        var result = JsonUtils.DeserializeOrDefault<TileEntity>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-tile"));
    }

    [Test]
    public void DeserializeOrDefault_InvalidJson_ReturnsDefault()
    {
        var invalidJson = "{ invalid json }";
        var defaultValue = new TileEntity { Id = "default" };

        var result = JsonUtils.DeserializeOrDefault(invalidJson, defaultValue);

        Assert.That(result, Is.EqualTo(defaultValue));
        Assert.That(result.Id, Is.EqualTo("default"));
    }

    [Test]
    public void DeserializeOrDefault_NullOrEmptyJson_ReturnsDefault()
    {
        var defaultValue = new TileEntity { Id = "default" };

        var result1 = JsonUtils.DeserializeOrDefault(null, defaultValue);
        var result2 = JsonUtils.DeserializeOrDefault("", defaultValue);
        var result3 = JsonUtils.DeserializeOrDefault("   ", defaultValue);

        Assert.That(result1, Is.EqualTo(defaultValue));
        Assert.That(result2, Is.EqualTo(defaultValue));
        Assert.That(result3, Is.EqualTo(defaultValue));
    }

    [Test]
    public void IsValidJson_ValidJson_ReturnsTrue()
    {
        var validJson = """{ "test": "value" }""";

        var result = JsonUtils.IsValidJson(validJson);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidJson_InvalidJson_ReturnsFalse()
    {
        var invalidJson = "{ invalid json }";

        var result = JsonUtils.IsValidJson(invalidJson);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidJson_NullOrEmptyJson_ReturnsFalse()
    {
        Assert.That(JsonUtils.IsValidJson(null), Is.False);
        Assert.That(JsonUtils.IsValidJson(""), Is.False);
        Assert.That(JsonUtils.IsValidJson("   "), Is.False);
    }

    [Test]
    public void RemoveJsonConverter_ExistingConverter_ReturnsTrue()
    {
        var testConverter = new JsonStringEnumConverter();
        JsonUtils.AddJsonConverter(testConverter);

        var removed = JsonUtils.RemoveJsonConverter<JsonStringEnumConverter>();

        Assert.That(removed, Is.True);
    }

    [Test]
    public void RemoveJsonConverter_NonExistentConverter_ReturnsFalse()
    {
        var removed = JsonUtils.RemoveJsonConverter<JsonConverter<DateTime>>();

        Assert.That(removed, Is.False);
    }

    [Test]
    public void GetJsonConverters_ReturnsReadOnlyList()
    {
        var converters = JsonUtils.GetJsonConverters();

        Assert.That(converters, Is.Not.Null);
        Assert.That(converters, Is.TypeOf<ReadOnlyCollection<JsonConverter>>());
    }

    [Test]
    public async Task DeserializeFromStreamAsync_ValidStream_ReturnsObject()
    {
        var testObject = new TileEntity
        {
            Id = "test-tile",
            Name = "Test Tile",
            Glyph = "@",
            BackgroundColor = "#000000",
            ForegroundColor = "#FFFFFF"
        };

        var json = JsonUtils.Serialize(testObject);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var result = await JsonUtils.DeserializeFromStreamAsync<TileEntity>(stream);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-tile"));
    }

    [Test]
    public async Task DeserializeFromStreamAsync_NullStream_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await JsonUtils.DeserializeFromStreamAsync<TileEntity>(null!)
        );
    }

    [Test]
    public void SerializeMultipleToDirectory_ValidObjects_CreatesFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var objects = new Dictionary<string, TileEntity>
        {
            ["tile1"] = new()
                { Id = "tile1", Name = "Tile 1", Glyph = "@", BackgroundColor = "#000", ForegroundColor = "#FFF" },
            ["tile2"] = new()
                { Id = "tile2", Name = "Tile 2", Glyph = "#", BackgroundColor = "#111", ForegroundColor = "#EEE" }
        };

        try
        {
            JsonUtils.SerializeMultipleToDirectory(objects, tempDir);

            Assert.That(Directory.Exists(tempDir), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "tile1.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(tempDir, "tile2.json")), Is.True);

            var content1 = File.ReadAllText(Path.Combine(tempDir, "tile1.json"));
            Assert.That(content1, Does.Contain("Tile 1"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Test]
    public void SerializeMultipleToDirectory_NullObjects_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JsonUtils.SerializeMultipleToDirectory<TileEntity>(null!, "test")
        );
    }

    [Test]
    public void SerializeMultipleToDirectory_NullDirectory_ThrowsArgumentNullException()
    {
        var objects = new Dictionary<string, TileEntity>();

        Assert.Throws<ArgumentNullException>(() =>
            JsonUtils.SerializeMultipleToDirectory(objects, null!)
        );
    }

    [Test]
    public void Serialize_WithCustomOptions_UsesProvidedOptions()
    {
        var testObject = new { Test = "value" };
        var customOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = null
        };

        var result = JsonUtils.Serialize(testObject, customOptions);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Not.Contain("\n")); // Not indented
        Assert.That(result, Does.Contain("Test"));   // Preserved casing
    }

    [Test]
    public void Serialize_WithCustomOptions_NullOptions_ThrowsArgumentNullException()
    {
        var testObject = new { Test = "value" };

        Assert.Throws<ArgumentNullException>(() =>
            JsonUtils.Serialize(testObject, null!)
        );
    }

    #endregion

    #region Test Data Classes

    // Test class for PascalCase conversion
    private class MyComplexTypeEntity
    {
    }

    private class MyVeryComplexTypeName
    {
    }

    private class SimpleEntity
    {
    }

    private class TestType
    {
    }

    private class TestEmptyType
    {
    }

    #endregion
}
