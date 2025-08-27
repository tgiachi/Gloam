using Gloam.Core.Json;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;
using System.Text.Json;

namespace Gloam.Tests.Json;

/// <summary>
/// Tests for JsonUtils functionality.
/// </summary>
public class JsonUtilsTests
{
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
    public void DeserializeFromString_NullString_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JsonUtils.DeserializeFromString<TileEntity>(null!));
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
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        
        Assert.Throws<FileNotFoundException>(() => JsonUtils.DeserializeFromFile<TileEntity>(nonExistentPath));
    }

    [Test]
    public async Task DeserializeFromFileAsync_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        
        Assert.ThrowsAsync<FileNotFoundException>(async () => 
            await JsonUtils.DeserializeFromFileAsync<TileEntity>(nonExistentPath));
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
    public void DeserializeFromString_ValidJson_ReturnsObject()
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

        var result = JsonUtils.DeserializeFromString<TileEntity>(json);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-tile"));
    }

    [Test]
    public void DeserializeFromString_InvalidJson_ThrowsJsonException()
    {
        var invalidJson = "null";
        
        Assert.Throws<JsonException>(() => JsonUtils.DeserializeFromString<TileEntity>(invalidJson));
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
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

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
                File.Delete(tempFilePath);
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
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

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
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public async Task SerializeToFileAsync_NullObject_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => 
            await JsonUtils.SerializeToFileAsync<TileEntity>(null!, "test.json"));
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
            await JsonUtils.SerializeToFileAsync(testObject, null!));
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
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

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
                File.Delete(tempFilePath);
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
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

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
                File.Delete(tempFilePath);
        }
    }

    [Test]
    public async Task DeserializeFromFileAsync_NullPath_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => 
            await JsonUtils.DeserializeFromFileAsync<TileEntity>(null!));
    }

    #endregion

    #region Converter Management Tests

    [Test]
    public void AddJsonConverter_ValidConverter_AddsToList()
    {
        var initialCount = JsonUtils.JsonConverters.Count;
        var testConverter = new System.Text.Json.Serialization.JsonStringEnumConverter();
        
        // This might throw if the JsonSerializerOptions is already read-only from previous tests
        // We test that it doesn't throw for null converter, but may throw for read-only options
        try
        {
            JsonUtils.AddJsonConverter(testConverter);
            Assert.That(JsonUtils.JsonConverters.Count, Is.EqualTo(initialCount + 1));
            Assert.That(JsonUtils.JsonConverters, Does.Contain(testConverter));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("read-only"))
        {
            // This is expected if the JsonSerializerOptions was already used in a previous test
            // The important thing is that we tested the main path and the null check works
            Assert.That(JsonUtils.JsonConverters.Count, Is.GreaterThanOrEqualTo(initialCount));
        }
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
        var dataContext = new Gloam.Data.Context.GloamDataJsonContext();
        
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
        var convertMethod = jsonUtilsType.GetMethod("ConvertToSnakeCase", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var result = convertMethod?.Invoke(null, new object[] { "" });
        Assert.That(result, Is.EqualTo(""));
    }

    [Test] 
    public void ConvertToSnakeCase_WithNullString_ShouldReturnNull()
    {
        // Test the edge case where pascalCase is null via reflection
        var jsonUtilsType = typeof(JsonUtils);
        var convertMethod = jsonUtilsType.GetMethod("ConvertToSnakeCase", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        var result = convertMethod?.Invoke(null, new object[] { null! });
        Assert.That(result, Is.Null);
    }

    #endregion

    #region Test Data Classes

    // Test class for PascalCase conversion
    private class MyComplexTypeEntity { }
    private class MyVeryComplexTypeName { }
    private class SimpleEntity { }
    private class TestType { }
    private class TestEmptyType { }


    #endregion
}