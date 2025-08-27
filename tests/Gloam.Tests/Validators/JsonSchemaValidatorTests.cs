using Gloam.Data.Validators;

namespace Gloam.Tests.Validators;

/// <summary>
///     Tests for JsonSchemaValidator functionality.
/// </summary>
public class JsonSchemaValidatorTests
{
    private JsonSchemaValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new JsonSchemaValidator();
    }

    #region Constructor Tests

    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        Assert.DoesNotThrow(() => new JsonSchemaValidator());
    }

    #endregion

    #region Validation Success Tests

    [Test]
    public void Validate_WithValidEntityAndSchema_ShouldReturnSuccess()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "name": { "type": "string" },
                             "age": { "type": "number" }
                         },
                         "required": ["name"]
                     }
                     """;

        var entity = """
                     {
                         "name": "John Doe",
                         "age": 30
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_WithMinimalValidEntity_ShouldReturnSuccess()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "id": { "type": "string" }
                         },
                         "required": ["id"]
                     }
                     """;

        var entity = """
                     {
                         "id": "test-123"
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_WithTrailingCommasInEntity_ShouldReturnSuccess()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "name": { "type": "string" }
                         }
                     }
                     """;

        var entity = """
                     {
                         "name": "Test",
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    [Test]
    public void Validate_WithComplexNestedSchema_ShouldReturnSuccess()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "user": {
                                 "type": "object",
                                 "properties": {
                                     "name": { "type": "string" },
                                     "contacts": {
                                         "type": "array",
                                         "items": {
                                             "type": "object",
                                             "properties": {
                                                 "type": { "type": "string" },
                                                 "value": { "type": "string" }
                                             }
                                         }
                                     }
                                 }
                             }
                         }
                     }
                     """;

        var entity = """
                     {
                         "user": {
                             "name": "Alice",
                             "contacts": [
                                 { "type": "email", "value": "alice@example.com" },
                                 { "type": "phone", "value": "+1234567890" }
                             ]
                         }
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    #endregion

    #region Validation Failure Tests

    [Test]
    public void Validate_WithMissingRequiredProperty_ShouldReturnFailure()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "name": { "type": "string" },
                             "id": { "type": "string" }
                         },
                         "required": ["id"]
                     }
                     """;

        var entity = """
                     {
                         "name": "John Doe"
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
        Assert.That(result.Errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Validate_WithWrongPropertyType_ShouldReturnFailure()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "age": { "type": "number" }
                         }
                     }
                     """;

        var entity = """
                     {
                         "age": "thirty"
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    [Test]
    public void Validate_WithArrayValidationFailure_ShouldReturnFailure()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "numbers": {
                                 "type": "array",
                                 "items": { "type": "number" }
                             }
                         }
                     }
                     """;

        var entity = """
                     {
                         "numbers": [1, 2, "three", 4]
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    [Test]
    public void Validate_WithStringLengthConstraint_ShouldReturnFailure()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "name": { 
                                 "type": "string",
                                 "minLength": 3,
                                 "maxLength": 10
                             }
                         }
                     }
                     """;

        var entity = """
                     {
                         "name": "A very long name that exceeds the maximum length"
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    [Test]
    public void Validate_WithNumericRangeConstraint_ShouldReturnFailure()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "score": { 
                                 "type": "number",
                                 "minimum": 0,
                                 "maximum": 100
                             }
                         }
                     }
                     """;

        var entity = """
                     {
                         "score": 150
                     }
                     """;

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    #endregion

    #region Input Validation Tests

    [Test]
    public void Validate_WithNullEntity_ShouldThrowArgumentException()
    {
        var schema = """{ "type": "object" }""";

        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!, schema));
    }

    [Test]
    public void Validate_WithEmptyEntity_ShouldThrowArgumentException()
    {
        var schema = """{ "type": "object" }""";

        Assert.Throws<ArgumentException>(() => _validator.Validate("", schema));
    }

    [Test]
    public void Validate_WithWhitespaceEntity_ShouldThrowArgumentException()
    {
        var schema = """{ "type": "object" }""";

        Assert.Throws<ArgumentException>(() => _validator.Validate("   ", schema));
    }

    [Test]
    public void Validate_WithNullSchema_ShouldThrowArgumentException()
    {
        var entity = """{ "test": "value" }""";

        Assert.Throws<ArgumentNullException>(() => _validator.Validate(entity, null!));
    }

    [Test]
    public void Validate_WithEmptySchema_ShouldThrowArgumentException()
    {
        var entity = """{ "test": "value" }""";

        Assert.Throws<ArgumentException>(() => _validator.Validate(entity, ""));
    }

    [Test]
    public void Validate_WithWhitespaceSchema_ShouldThrowArgumentException()
    {
        var entity = """{ "test": "value" }""";

        Assert.Throws<ArgumentException>(() => _validator.Validate(entity, "   "));
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public void Validate_WithInvalidEntityJson_ShouldReturnFailureWithErrorMessage()
    {
        var schema = """{ "type": "object" }""";
        var invalidEntity = """{ "name": "John", "incomplete": }""";

        var result = _validator.Validate(invalidEntity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
        Assert.That(result.Errors.First(), Does.StartWith("Invalid JSON format:"));
    }

    [Test]
    public void Validate_WithInvalidSchemaJson_ShouldReturnFailureWithErrorMessage()
    {
        var entity = """{ "name": "John" }""";
        var invalidSchema = """{ "type": "object", "properties": { "name": { "type": } }""";

        var result = _validator.Validate(entity, invalidSchema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
        Assert.That(result.Errors.First(), Does.Contain("JSON"));
    }

    [Test]
    public void Validate_WithMalformedJson_ShouldReturnFailureWithSpecificMessage()
    {
        var schema = """{ "type": "object" }""";
        var malformedEntity = """{ name: "John" }"""; // Missing quotes around property name

        var result = _validator.Validate(malformedEntity, schema);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
        Assert.That(result.Errors.First(), Does.StartWith("Invalid JSON format:"));
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public void Validate_WithEmptyObjectAgainstEmptySchema_ShouldReturnSuccess()
    {
        var schema = """{}""";
        var entity = """{}""";

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    [Test]
    public void Validate_WithArrayEntity_ShouldWork()
    {
        var schema = """
                     {
                         "type": "array",
                         "items": { "type": "string" }
                     }
                     """;

        var entity = """["item1", "item2", "item3"]""";

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    [Test]
    public void Validate_WithPrimitiveEntity_ShouldWork()
    {
        var schema = """{ "type": "string" }""";
        var entity = "\"Hello World\"";

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    [Test]
    public void Validate_WithBooleanEntity_ShouldWork()
    {
        var schema = """{ "type": "boolean" }""";
        var entity = """true""";

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    [Test]
    public void Validate_WithNullEntity_ShouldWork()
    {
        var schema = """{ "type": "null" }""";
        var entity = """null""";

        var result = _validator.Validate(entity, schema);

        Assert.That(result.Ok, Is.True);
    }

    #endregion

    #region Complex Schema Tests

    [Test]
    public void Validate_WithEnumConstraint_ShouldWork()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "status": {
                                 "type": "string",
                                 "enum": ["active", "inactive", "pending"]
                             }
                         }
                     }
                     """;

        var validEntity = """{ "status": "active" }""";
        var invalidEntity = """{ "status": "unknown" }""";

        var validResult = _validator.Validate(validEntity, schema);
        var invalidResult = _validator.Validate(invalidEntity, schema);

        Assert.That(validResult.Ok, Is.True);
        Assert.That(invalidResult.Ok, Is.False);
    }

    [Test]
    public void Validate_WithPatternConstraint_ShouldWork()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "email": {
                                 "type": "string",
                                 "pattern": "^[^@]+@[^@]+\\.[^@]+$"
                             }
                         }
                     }
                     """;

        var validEntity = """{ "email": "test@example.com" }""";
        var invalidEntity = """{ "email": "invalid-email" }""";

        var validResult = _validator.Validate(validEntity, schema);
        var invalidResult = _validator.Validate(invalidEntity, schema);

        Assert.That(validResult.Ok, Is.True);
        Assert.That(invalidResult.Ok, Is.False);
    }

    [Test]
    public void Validate_WithConditionalSchema_ShouldWork()
    {
        var schema = """
                     {
                         "type": "object",
                         "properties": {
                             "type": { "type": "string" },
                             "data": {}
                         },
                         "if": {
                             "properties": { "type": { "const": "user" } }
                         },
                         "then": {
                             "properties": {
                                 "data": {
                                     "type": "object",
                                     "properties": {
                                         "name": { "type": "string" }
                                     },
                                     "required": ["name"]
                                 }
                             }
                         }
                     }
                     """;

        var validEntity = """
                          {
                              "type": "user",
                              "data": { "name": "John Doe" }
                          }
                          """;

        var result = _validator.Validate(validEntity, schema);
        Assert.That(result.Ok, Is.True);
    }

    #endregion
}
