using System.ComponentModel.DataAnnotations;

namespace Gloam.Data.Interfaces.Validation;

/// <summary>
/// Provides functionality for validating entities against schemas.
/// </summary>
public interface IEntitySchemaValidator
{
    /// <summary>
    /// Validates an entity against a schema.
    /// </summary>
    /// <param name="entity">The entity data to validate.</param>
    /// <param name="schema">The schema to validate against.</param>
    /// <returns>A ValidationResult indicating the outcome of the validation.</returns>
    ValidationResult Validate(string entity, string schema);
}
