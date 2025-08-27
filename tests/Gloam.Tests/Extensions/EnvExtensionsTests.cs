namespace Gloam.Tests.Extensions;

public class EnvExtensionsTests
{
    [Test]
    public void GetEnvironmentVariable_WithExistingVariable_ShouldReturnValue()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        Assert.That(path, Is.Not.Null);
        Assert.That(path, Is.Not.Empty);
    }

    [Test]
    public void GetEnvironmentVariable_WithNonExistingVariable_ShouldReturnNull()
    {
        var result = Environment.GetEnvironmentVariable("GLOAM_NONEXISTENT_VAR_12345");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void SetEnvironmentVariable_ShouldSetValue()
    {
        var testKey = "GLOAM_TEST_VAR";
        var testValue = "test_value_123";

        try
        {
            Environment.SetEnvironmentVariable(testKey, testValue);
            var result = Environment.GetEnvironmentVariable(testKey);
            Assert.That(result, Is.EqualTo(testValue));
        }
        finally
        {
            Environment.SetEnvironmentVariable(testKey, null);
        }
    }

    [Test]
    public void GetEnvironmentVariable_WithDefaultValue_ShouldReturnDefault()
    {
        var result = Environment.GetEnvironmentVariable("GLOAM_NONEXISTENT_VAR_12345") ?? "default_value";
        Assert.That(result, Is.EqualTo("default_value"));
    }
}
