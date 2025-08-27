using Gloam.Data.Entities.Base;
using Gloam.Data.Objects;

namespace Gloam.Data.Interfaces.Validation;

/// <summary>
///     Provides functionality for validating entities against schemas.
/// </summary>
public interface IEntitySchemaValidator
{
    /// <summary>
    ///     Validates an entity against a schema.
    /// </summary>
    /// <param name="entity">The entity data to validate.</param>
    /// <param name="schema">The schema to validate against.</param>
    /// <returns>A ValidationResult indicating the outcome of the validation.</returns>
    ValidationResult Validate(string entity, string schema);


    /// <summary>
    ///     Generates a JSON schema for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to generate a schema for</typeparam>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A JSON schema string for the specified entity type</returns>
    Task<string> GetSchemaAsync<T>(CancellationToken cancellationToken = default) where T : BaseGloamEntity;
}
