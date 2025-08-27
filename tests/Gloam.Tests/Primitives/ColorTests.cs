using Gloam.Core.Primitives;

namespace Gloam.Tests.Primitives;

/// <summary>
///     Tests for the Color struct.
/// </summary>
public class ColorTests
{
    [Test]
    public void Constructor_WithValidValues_ShouldInitializeCorrectly()
    {
        var color = new Color(128, 64, 192);

        Assert.That(color.R, Is.EqualTo(128));
        Assert.That(color.G, Is.EqualTo(64));
        Assert.That(color.B, Is.EqualTo(192));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void Constructor_WithDefaultAlpha_ShouldUse255()
    {
        var color = new Color(100, 150, 200);

        Assert.That(color.R, Is.EqualTo(100));
        Assert.That(color.G, Is.EqualTo(150));
        Assert.That(color.B, Is.EqualTo(200));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void Constructor_WithValuesOverMax_ShouldClampTo255()
    {
        var color = new Color(300, 400, 500, 600);

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(255));
        Assert.That(color.B, Is.EqualTo(255));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void Constructor_WithNegativeValues_ShouldClampToZero()
    {
        var color = new Color(-50, -100, -150, -200);

        Assert.That(color.R, Is.EqualTo(0));
        Assert.That(color.G, Is.EqualTo(0));
        Assert.That(color.B, Is.EqualTo(0));
        Assert.That(color.A, Is.EqualTo(0));
    }

    [Test]
    public void FromHex_WithValidSixCharacterHex_ShouldParseCorrectly()
    {
        var color = Color.FromHex("FF8040");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void FromHex_WithHashPrefix_ShouldParseCorrectly()
    {
        var color = Color.FromHex("#FF8040");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void FromHex_WithEightCharacterHex_ShouldParseWithAlpha()
    {
        var color = Color.FromHex("FF804080");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
        Assert.That(color.A, Is.EqualTo(128));
    }

    [Test]
    public void FromHex_WithEightCharacterHexAndHash_ShouldParseWithAlpha()
    {
        var color = Color.FromHex("#FF804080");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
        Assert.That(color.A, Is.EqualTo(128));
    }

    [Test]
    public void FromHex_WithLowercaseHex_ShouldParseCorrectly()
    {
        var color = Color.FromHex("ff8040");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(128));
        Assert.That(color.B, Is.EqualTo(64));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void FromHex_WithNullHex_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Color.FromHex(null!));
    }

    [Test]
    public void FromHex_WithEmptyHex_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Color.FromHex(""));
    }

    [Test]
    public void FromHex_WithWhitespaceHex_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Color.FromHex("   "));
    }

    [Test]
    public void FromHex_WithInvalidLength_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Color.FromHex("FF80"));
        Assert.Throws<ArgumentException>(() => Color.FromHex("FF8040A"));
        Assert.Throws<ArgumentException>(() => Color.FromHex("FF8040AA1"));
    }

    [Test]
    public void FromHex_WithInvalidCharacters_ShouldThrowFormatException()
    {
        Assert.Throws<FormatException>(() => Color.FromHex("GGHHII"));
    }

    [Test]
    public void FromHex_WithValidBlackColor_ShouldParseCorrectly()
    {
        var color = Color.FromHex("000000");

        Assert.That(color.R, Is.EqualTo(0));
        Assert.That(color.G, Is.EqualTo(0));
        Assert.That(color.B, Is.EqualTo(0));
        Assert.That(color.A, Is.EqualTo(255));
    }

    [Test]
    public void FromHex_WithValidWhiteColor_ShouldParseCorrectly()
    {
        var color = Color.FromHex("FFFFFF");

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(255));
        Assert.That(color.B, Is.EqualTo(255));
        Assert.That(color.A, Is.EqualTo(255));
    }
}
