namespace Gloam.Data.Objects;

/// <summary>
///     Represents the result of a validation operation with success status and error messages.
/// </summary>
/// <param name="Ok">True if validation succeeded, false otherwise.</param>
/// <param name="Errors">Collection of error messages if validation failed.</param>
public record ValidationResult(bool Ok, IReadOnlyCollection<string> Errors);
