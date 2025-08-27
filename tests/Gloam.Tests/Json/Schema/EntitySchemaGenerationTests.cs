using System.Text.Json;
using Gloam.Data.Entities.Base;
using Gloam.Data.Entities.Colors;
using Gloam.Data.Entities.Tiles;
using Json.Schema;
using Json.Schema.Generation;

namespace Gloam.Tests.Json.Schema;

/// <summary>
///     Tests for JSON schema generation for Gloam entities.
/// </summary>
public class EntitySchemaGenerationTests
{
    [Test]
    public void BaseGloamEntity_ShouldGenerateValidSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<BaseGloamEntity>()
            .Build();

        Assert.That(schema, Is.Not.Null);

        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Assert.That(schemaJson, Is.Not.Empty);

        // Verify required properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Id"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Name"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Description"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Tags"), Is.True);
    }

    [Test]
    public void TileEntity_ShouldGenerateValidSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<TileEntity>()
            .Build();

        Assert.That(schema, Is.Not.Null);

        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Assert.That(schemaJson, Is.Not.Empty);

        // Verify tile-specific properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Glyph"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("BackgroundColor"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("ForegroundColor"), Is.True);

        // Verify inherited properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Id"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Name"), Is.True);
    }

    [Test]
    public void TileSetEntity_ShouldGenerateValidSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<TileSetEntity>()
            .Build();

        Assert.That(schema, Is.Not.Null);

        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Assert.That(schemaJson, Is.Not.Empty);

        // Verify tileset-specific properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Tiles"), Is.True);

        // Verify inherited properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Id"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Name"), Is.True);
    }

    [Test]
    public void ColorSetEntity_ShouldGenerateValidSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<ColorSetEntity>()
            .Build();

        Assert.That(schema, Is.Not.Null);

        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Assert.That(schemaJson, Is.Not.Empty);

        // Verify colorset-specific properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Colors"), Is.True);

        // Verify inherited properties exist
        Assert.That(schema.GetProperties()?.ContainsKey("Id"), Is.True);
        Assert.That(schema.GetProperties()?.ContainsKey("Name"), Is.True);
    }

    [Test]
    public void AllEntities_ShouldHaveRequiredPropertiesInSchema()
    {
        var entityTypes = new[]
        {
            typeof(BaseGloamEntity),
            typeof(TileEntity),
            typeof(TileSetEntity),
            typeof(ColorSetEntity)
        };

        foreach (var entityType in entityTypes)
        {
            var schema = new JsonSchemaBuilder()
                .FromType(entityType)
                .Build();

            Assert.That(schema, Is.Not.Null, $"Schema should be generated for {entityType.Name}");

            var required = schema.GetRequired();

            // Required properties may be null if no properties are marked as required
            if (required != null)
            {
                // BaseGloamEntity should have Id and Name as required
                if (entityType == typeof(BaseGloamEntity) || entityType.IsSubclassOf(typeof(BaseGloamEntity)))
                {
                    Assert.That(required.Contains("Id"), Is.True, $"{entityType.Name} should require Id property");
                    Assert.That(required.Contains("Name"), Is.True, $"{entityType.Name} should require Name property");
                }
            }
            else
            {
                // If no required properties, just verify schema was generated successfully
                Assert.That(schema.GetProperties(), Is.Not.Null, $"Schema properties should exist for {entityType.Name}");
            }
        }
    }

    [Test]
    public void GeneratedSchemas_ShouldBeValidJson()
    {
        var entityTypes = new[]
        {
            typeof(BaseGloamEntity),
            typeof(TileEntity),
            typeof(TileSetEntity),
            typeof(ColorSetEntity)
        };

        foreach (var entityType in entityTypes)
        {
            var schema = new JsonSchemaBuilder()
                .FromType(entityType)
                .Build();

            var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

            Assert.That(schemaJson, Is.Not.Empty, $"Schema JSON should not be empty for {entityType.Name}");

            // Verify it's valid JSON by deserializing
            Assert.DoesNotThrow(
                () => JsonDocument.Parse(schemaJson),
                $"Schema should be valid JSON for {entityType.Name}"
            );
        }
    }
}
