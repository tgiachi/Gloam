using System.Text.Json;
using System.Text.Json.Serialization;
using Gloam.Core.Json;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;
using Json.Schema;
using Json.Schema.Generation;

namespace Gloam.Tests.Json.Schema;

/// <summary>
///     Generates JSON schema files for Gloam entities.
/// </summary>
public class SchemaFileGenerator
{
    private static readonly string OutputDirectory = "schemas";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Test]
    public async Task GenerateAllEntitySchemas()
    {
        var entityTypes = new[]
        {
            typeof(BaseGloamEntity),
            typeof(TileEntity),
            typeof(TileSetEntity),
            typeof(ColorSetEntity)
        };

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(OutputDirectory);

        foreach (var entityType in entityTypes)
        {
            var fileName = JsonUtils.GetSchemaFileName(entityType);
            await GenerateSchemaFile(entityType, fileName);
        }

        // Verify all files were created
        foreach (var entityType in entityTypes)
        {
            var fileName = JsonUtils.GetSchemaFileName(entityType);
            var filePath = Path.Combine(OutputDirectory, fileName);
            Assert.That(File.Exists(filePath), Is.True, $"Schema file should exist: {fileName}");
        }
    }

    private static async Task GenerateSchemaFile(Type entityType, string fileName)
    {
        var schema = new JsonSchemaBuilder()
            .FromType(entityType)
            .Build();

        var schemaJson = JsonSerializer.Serialize(schema, JsonOptions);

        var filePath = Path.Combine(OutputDirectory, fileName);
        await File.WriteAllTextAsync(filePath, schemaJson);

        Console.WriteLine($"Generated schema file: {filePath}");
    }

    [Test]
    public async Task GenerateBaseGloamEntitySchema()
    {
        Directory.CreateDirectory(OutputDirectory);
        var fileName = JsonUtils.GetSchemaFileName(typeof(BaseGloamEntity));
        await GenerateSchemaFile(typeof(BaseGloamEntity), fileName);
    }

    [Test]
    public async Task GenerateTileEntitySchema()
    {
        Directory.CreateDirectory(OutputDirectory);
        var fileName = JsonUtils.GetSchemaFileName(typeof(TileEntity));
        await GenerateSchemaFile(typeof(TileEntity), fileName);
    }

    [Test]
    public async Task GenerateTileSetEntitySchema()
    {
        Directory.CreateDirectory(OutputDirectory);
        var fileName = JsonUtils.GetSchemaFileName(typeof(TileSetEntity));
        await GenerateSchemaFile(typeof(TileSetEntity), fileName);
    }

    [Test]
    public async Task GenerateColorSetEntitySchema()
    {
        Directory.CreateDirectory(OutputDirectory);
        var fileName = JsonUtils.GetSchemaFileName(typeof(ColorSetEntity));
        await GenerateSchemaFile(typeof(ColorSetEntity), fileName);
    }
}
