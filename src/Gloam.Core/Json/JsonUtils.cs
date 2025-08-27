using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Gloam.Core.Json;

public static class JsonUtils
{
    private static readonly List<IJsonTypeInfoResolver> JsonSerializerContexts = new(8);

    private static JsonSerializerOptions _jsonSerializerOptions = null!;

    static JsonUtils()
    {
        RebuildJsonSerializerContexts();
    }

    public static List<JsonConverter> JsonConverters { get; } = new(8)
    {
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    };

    public static void AddJsonConverter(JsonConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        JsonConverters.Add(converter);
        _jsonSerializerOptions.Converters.Add(converter);
    }

    private static void RebuildJsonSerializerContexts()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(JsonSerializerContexts.ToArray())
        };

        foreach (var converter in JsonConverters)
        {
            _jsonSerializerOptions.Converters.Add(converter);
        }
    }

    public static void RegisterJsonContext(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        JsonSerializerContexts.Add(context);
        RebuildJsonSerializerContexts();
    }


    public static string Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return JsonSerializer.Serialize(obj, typeof(T), _jsonSerializerOptions);
    }

    public static T Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions) ??
               throw new JsonException("Deserialization failed.");
    }

    public static T DeserializeFromString<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions) ??
               throw new JsonException("Deserialization failed.");
    }

    public static T DeserializeFromFile<T>(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");
        }

        var json = File.ReadAllText(filePath);
        return Deserialize<T>(json);
    }


    public static void SerializeToFile<T>(T obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(filePath);

        var json = Serialize(obj);
        File.WriteAllText(filePath, json);
    }

    public static async Task SerializeToFileAsync<T>(T obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(filePath);

        var json = Serialize(obj);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    public static async Task<T> DeserializeFromFileAsync<T>(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");
        }

        var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        return Deserialize<T>(json);
    }

    /// <summary>
    /// Generates a schema file name for the given type using snake_case convention.
    /// </summary>
    /// <param name="type">The type to generate a schema name for.</param>
    /// <returns>A schema file name in the format "type_name.schema.json".</returns>
    public static string GetSchemaFileName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var typeName = type.Name;
        
        // Remove "Entity" suffix if present
        if (typeName.EndsWith("Entity"))
        {
            typeName = typeName[..^6]; // Remove last 6 characters ("Entity")
        }

        // Convert PascalCase to snake_case
        var snakeCaseName = ConvertToSnakeCase(typeName);
        
        return $"{snakeCaseName}.schema.json";
    }

    private static string ConvertToSnakeCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(pascalCase[0]));

        for (int i = 1; i < pascalCase.Length; i++)
        {
            if (char.IsUpper(pascalCase[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(pascalCase[i]));
            }
            else
            {
                result.Append(pascalCase[i]);
            }
        }

        return result.ToString();
    }
}
