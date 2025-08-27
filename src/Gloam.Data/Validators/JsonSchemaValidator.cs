using System.Text.Json;
using System.Text.Json.Serialization;
using Gloam.Data.Entities.Base;
using Gloam.Data.Interfaces.Validation;
using Json.Schema;
using Json.Schema.Generation;
using ValidationResult = Gloam.Data.Objects.ValidationResult;

namespace Gloam.Data.Validators;

/// <summary>
///     Validates JSON entities against JSON Schema specifications.
/// </summary>
public class JsonSchemaValidator : IEntitySchemaValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    private readonly Dictionary<Type, string> _schemas = new();

    /// <summary>
    ///     Validates an entity JSON string against a JSON Schema.
    /// </summary>
    /// <param name="entity">The JSON string representation of the entity to validate.</param>
    /// <param name="schema">The JSON Schema string to validate against.</param>
    /// <returns>ValidationResult indicating success or failure with error messages.</returns>
    public ValidationResult Validate(string entity, string schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        try
        {
            // Parse the JSON Schema
            var jsonSchema = JsonSchema.FromText(schema);

            // Parse the entity JSON
            JsonDocument entityDocument;
            try
            {
                entityDocument = JsonDocument.Parse(entity, DocumentOptions);
            }
            catch (JsonException ex)
            {
                return new ValidationResult(false, [$"Invalid JSON format: {ex.Message}"]);
            }

            using (entityDocument)
            {
                // Validate the entity against the schema
                var validationResults = jsonSchema.Evaluate(entityDocument.RootElement);

                if (validationResults.IsValid)
                {
                    return new ValidationResult(true, []);
                }

                // Collect validation errors
                var errors = CollectValidationErrors(validationResults).ToList();
                return new ValidationResult(false, errors.AsReadOnly());
            }
        }
        catch (JsonException ex)
        {
            return new ValidationResult(false, new[] { $"Invalid JSON Schema format: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, new[] { $"Validation error: {ex.Message}" });
        }
    }

    /// <summary>
    ///     Generates a schema for the specified entity type.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<string> GetSchemaAsync<T>(CancellationToken cancellationToken = default) where T : BaseGloamEntity
    {
        if (_schemas.TryGetValue(typeof(T), out var cachedSchema))
        {
            return cachedSchema;
        }

        var schema = new JsonSchemaBuilder()
            .FromType<T>()
            .Build();

        var schemaJson = JsonSerializer.Serialize(schema, JsonOptions);

        _schemas[typeof(T)] = schemaJson;


        return schemaJson;
    }

    /// <summary>
    ///     Recursively collects validation error messages from the evaluation results.
    /// </summary>
    /// <param name="results">The validation results to process.</param>
    /// <returns>An enumerable of error messages.</returns>
    private static IEnumerable<string> CollectValidationErrors(EvaluationResults results)
    {
        if (!results.IsValid)
        {
            // Get error information from the current result
            var instanceLocation = results.InstanceLocation?.ToString() ?? "root";
            var schemaLocation = results.SchemaLocation?.ToString() ?? "unknown";

            // Check if there are any errors in the result
            if (results.Errors != null && results.Errors.Any())
            {
                foreach (var error in results.Errors)
                {
                    yield return $"At '{instanceLocation}' (schema: {schemaLocation}): {error.Value}";
                }
            }
            else if (!results.HasDetails)
            {
                // If no specific error message and no details, provide a generic message
                yield return $"Validation failed at '{instanceLocation}' (schema: {schemaLocation})";
            }

            // Process nested validation results
            foreach (var detail in results.Details)
            {
                foreach (var error in CollectValidationErrors(detail))
                {
                    yield return error;
                }
            }
        }
    }
}
