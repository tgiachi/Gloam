using Gloam.Data.Objects;

namespace Gloam.Tests.Objects;

/// <summary>
///     Tests for the ValidationResult record.
/// </summary>
public class ValidationResultTests
{
    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = new ValidationResult(true, errors);

        Assert.That(result.Ok, Is.True);
        Assert.That(result.Errors, Is.EqualTo(errors));
    }

    [Test]
    public void Constructor_WithEmptyErrors_ShouldWork()
    {
        var errors = new List<string>();
        var result = new ValidationResult(true, errors);

        Assert.That(result.Ok, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Constructor_WithNullErrors_ShouldWork()
    {
        var result = new ValidationResult(false, null!);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors, Is.Null);
    }

    [Test]
    public void SuccessResult_ShouldHaveOkTrueAndEmptyErrors()
    {
        var result = new ValidationResult(true, Array.Empty<string>());

        Assert.That(result.Ok, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void FailureResult_ShouldHaveOkFalseAndErrors()
    {
        var errors = new[] { "Validation failed", "Required field missing" };
        var result = new ValidationResult(false, errors);

        Assert.That(result.Ok, Is.False);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors, Contains.Item("Validation failed"));
        Assert.That(result.Errors, Contains.Item("Required field missing"));
    }

    [Test]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result1 = new ValidationResult(true, errors);
        var result2 = new ValidationResult(true, errors);

        Assert.That(result1.Equals(result2), Is.True);
        Assert.That(result1 == result2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentOkValues_ShouldReturnFalse()
    {
        var errors = new[] { "Error 1" };
        var result1 = new ValidationResult(true, errors);
        var result2 = new ValidationResult(false, errors);

        Assert.That(result1.Equals(result2), Is.False);
        Assert.That(result1 != result2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentErrors_ShouldReturnFalse()
    {
        var errors1 = new[] { "Error 1" };
        var errors2 = new[] { "Error 2" };
        var result1 = new ValidationResult(false, errors1);
        var result2 = new ValidationResult(false, errors2);

        Assert.That(result1.Equals(result2), Is.False);
        Assert.That(result1 != result2, Is.True);
    }

    [Test]
    public void GetHashCode_ForEqualResults_ShouldBeSame()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result1 = new ValidationResult(true, errors);
        var result2 = new ValidationResult(true, errors);

        Assert.That(result1.GetHashCode(), Is.EqualTo(result2.GetHashCode()));
    }

    [Test]
    public void ToString_ShouldReturnMeaningfulString()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result = new ValidationResult(false, errors);
        var toString = result.ToString();

        Assert.That(toString, Is.Not.Empty);
        Assert.That(toString, Contains.Substring("ValidationResult"));
        Assert.That(toString, Contains.Substring("False"));
    }

    [Test]
    public void Deconstruction_ShouldWork()
    {
        var errors = new[] { "Error 1" };
        var result = new ValidationResult(false, errors);

        var (ok, resultErrors) = result;

        Assert.That(ok, Is.False);
        Assert.That(resultErrors, Is.EqualTo(errors));
    }

    [Test]
    public void RecordEquality_WithSameReference_ShouldReturnTrue()
    {
        var errors = new[] { "Error 1" };
        var result = new ValidationResult(true, errors);
        var sameResult = result;

        Assert.That(result.Equals(sameResult), Is.True);
        Assert.That(ReferenceEquals(result, sameResult), Is.True);
    }

    [Test]
    public void RecordEquality_WithNull_ShouldReturnFalse()
    {
        var result = new ValidationResult(true, Array.Empty<string>());

        Assert.That(result.Equals(null), Is.False);
        Assert.That(result != null, Is.True);
    }

    [Test]
    public void Errors_ShouldBeReadOnly()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result = new ValidationResult(false, errors);

        // Should be IReadOnlyCollection, so no Add method
        Assert.That(result.Errors, Is.AssignableTo<IReadOnlyCollection<string>>());
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public void MultipleErrors_ShouldPreserveOrder()
    {
        var errors = new[] { "First error", "Second error", "Third error" };
        var result = new ValidationResult(false, errors);

        Assert.That(result.Errors.Count, Is.EqualTo(3));

        var errorList = result.Errors.ToList();
        Assert.That(errorList[0], Is.EqualTo("First error"));
        Assert.That(errorList[1], Is.EqualTo("Second error"));
        Assert.That(errorList[2], Is.EqualTo("Third error"));
    }

    [Test]
    public void Errors_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        var errors = new[] { "Error with \"quotes\"", "Error with 'apostrophes'", "Error with\nnewlines" };
        var result = new ValidationResult(false, errors);

        Assert.That(result.Errors.Count, Is.EqualTo(3));
        Assert.That(result.Errors, Contains.Item("Error with \"quotes\""));
        Assert.That(result.Errors, Contains.Item("Error with 'apostrophes'"));
        Assert.That(result.Errors, Contains.Item("Error with\nnewlines"));
    }

    [Test]
    public void Copy_WithDifferentOk_ShouldCreateNewInstance()
    {
        var errors = new[] { "Error 1" };
        var original = new ValidationResult(true, errors);
        var copy = original with { Ok = false };

        Assert.That(copy.Ok, Is.False);
        Assert.That(copy.Errors, Is.EqualTo(original.Errors));
        Assert.That(copy, Is.Not.EqualTo(original));
    }

    [Test]
    public void Copy_WithDifferentErrors_ShouldCreateNewInstance()
    {
        var originalErrors = new[] { "Error 1" };
        var newErrors = new[] { "Error 2" };
        var original = new ValidationResult(true, originalErrors);
        var copy = original with { Errors = newErrors };

        Assert.That(copy.Ok, Is.EqualTo(original.Ok));
        Assert.That(copy.Errors, Is.EqualTo(newErrors));
        Assert.That(copy, Is.Not.EqualTo(original));
    }
}
