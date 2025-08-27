using Gloam.Core.Extensions.Strings;

namespace Gloam.Tests.Extensions;

public class StringMethodExtensionTests
{
    [Test]
    public void ToSnakeCase_WithPascalCase_ShouldConvert()
    {
        var result = "HelloWorld".ToSnakeCase();
        Assert.That(result, Is.EqualTo("hello_world"));
    }

    [Test]
    public void ToSnakeCase_WithCamelCase_ShouldConvert()
    {
        var result = "helloWorld".ToSnakeCase();
        Assert.That(result, Is.EqualTo("hello_world"));
    }

    [Test]
    public void ToSnakeCase_WithSingleWord_ShouldLowercase()
    {
        var result = "Hello".ToSnakeCase();
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void ToSnakeCaseUpper_WithPascalCase_ShouldConvert()
    {
        var result = "HelloWorld".ToSnakeCaseUpper();
        Assert.That(result, Is.EqualTo("HELLO_WORLD"));
    }

    [Test]
    public void ToCamelCase_WithSnakeCase_ShouldConvert()
    {
        var result = "hello_world".ToCamelCase();
        Assert.That(result, Is.EqualTo("helloWorld"));
    }

    [Test]
    public void ToCamelCase_WithPascalCase_ShouldConvert()
    {
        var result = "HelloWorld".ToCamelCase();
        Assert.That(result, Is.EqualTo("helloWorld"));
    }

    [Test]
    public void ToPascalCase_WithSnakeCase_ShouldConvert()
    {
        var result = "hello_world".ToPascalCase();
        Assert.That(result, Is.EqualTo("HelloWorld"));
    }

    [Test]
    public void ToPascalCase_WithCamelCase_ShouldConvert()
    {
        var result = "helloWorld".ToPascalCase();
        Assert.That(result, Is.EqualTo("HelloWorld"));
    }

    [Test]
    public void ToKebabCase_WithPascalCase_ShouldConvert()
    {
        var result = "HelloWorld".ToKebabCase();
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void ToKebabCase_WithCamelCase_ShouldConvert()
    {
        var result = "helloWorld".ToKebabCase();
        Assert.That(result, Is.EqualTo("hello-world"));
    }

    [Test]
    public void ToTitleCase_WithLowercase_ShouldConvert()
    {
        var result = "hello world".ToTitleCase();
        Assert.That(result, Is.EqualTo("Hello World"));
    }

    [Test]
    public void ToTitleCase_WithSnakeCase_ShouldConvert()
    {
        var result = "hello_world_test".ToTitleCase();
        Assert.That(result, Is.EqualTo("Hello World Test"));
    }

    [Test]
    public void ChainedConversions_ShouldWork()
    {
        var result = "HelloWorldTest".ToSnakeCase().ToPascalCase();
        Assert.That(result, Is.EqualTo("HelloWorldTest"));
    }

    [Test]
    public void EmptyString_ShouldHandleGracefully()
    {
        var empty = "";
        Assert.That(empty.ToSnakeCase(), Is.EqualTo(""));
        Assert.That(empty.ToCamelCase(), Is.EqualTo(""));
        Assert.That(empty.ToPascalCase(), Is.EqualTo(""));
        Assert.That(empty.ToKebabCase(), Is.EqualTo(""));
        Assert.That(empty.ToTitleCase(), Is.EqualTo(""));
    }

    [Test]
    public void SingleCharacter_ShouldHandleCorrectly()
    {
        var single = "A";
        Assert.That(single.ToSnakeCase(), Is.EqualTo("a"));
        Assert.That(single.ToCamelCase(), Is.EqualTo("a"));
        Assert.That(single.ToPascalCase(), Is.EqualTo("A"));
        Assert.That(single.ToKebabCase(), Is.EqualTo("a"));
        Assert.That(single.ToTitleCase(), Is.EqualTo("A"));
    }
}
