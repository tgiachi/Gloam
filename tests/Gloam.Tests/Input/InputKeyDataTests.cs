using Gloam.Core.Input;

namespace Gloam.Tests.Input;

public class InputKeyDataTests
{
    [Test]
    public void Constructor_WithKeyCodeOnly_ShouldInitializeCorrectly()
    {
        var keyData = new InputKeyData(65);

        Assert.That(keyData.KeyCode, Is.EqualTo(65));
        Assert.That(keyData.Modifiers, Is.EqualTo(InputModifiers.None));
        Assert.That(keyData.Name, Is.EqualTo("65"));
    }

    [Test]
    public void Constructor_WithKeyCodeAndModifiers_ShouldInitializeCorrectly()
    {
        var keyData = new InputKeyData(65, InputModifiers.Ctrl);

        Assert.That(keyData.KeyCode, Is.EqualTo(65));
        Assert.That(keyData.Modifiers, Is.EqualTo(InputModifiers.Ctrl));
        Assert.That(keyData.Name, Is.EqualTo("65"));
    }

    [Test]
    public void Constructor_WithKeyCodeAndName_ShouldInitializeCorrectly()
    {
        var keyData = new InputKeyData(65, "A");

        Assert.That(keyData.KeyCode, Is.EqualTo(65));
        Assert.That(keyData.Modifiers, Is.EqualTo(InputModifiers.None));
        Assert.That(keyData.Name, Is.EqualTo("A"));
    }

    [Test]
    public void Constructor_WithAllParameters_ShouldInitializeCorrectly()
    {
        var keyData = new InputKeyData(65, InputModifiers.Shift | InputModifiers.Ctrl, "A");

        Assert.That(keyData.KeyCode, Is.EqualTo(65));
        Assert.That(keyData.Modifiers, Is.EqualTo(InputModifiers.Shift | InputModifiers.Ctrl));
        Assert.That(keyData.Name, Is.EqualTo("A"));
    }

    [Test]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var key1 = new InputKeyData(65, InputModifiers.Ctrl, "A");
        var key2 = new InputKeyData(65, InputModifiers.Ctrl, "A");

        Assert.That(key1.Equals(key2), Is.True);
        Assert.That(key1 == key2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentKeyCode_ShouldReturnFalse()
    {
        var key1 = new InputKeyData(65, InputModifiers.Ctrl, "A");
        var key2 = new InputKeyData(66, InputModifiers.Ctrl, "A");

        Assert.That(key1.Equals(key2), Is.False);
        Assert.That(key1 != key2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentModifiers_ShouldReturnFalse()
    {
        var key1 = new InputKeyData(65, InputModifiers.Ctrl, "A");
        var key2 = new InputKeyData(65, InputModifiers.Shift, "A");

        Assert.That(key1.Equals(key2), Is.False);
        Assert.That(key1 != key2, Is.True);
    }

    [Test]
    public void Equals_WithDifferentName_ShouldReturnTrue()
    {
        var key1 = new InputKeyData(65, InputModifiers.Ctrl, "A");
        var key2 = new InputKeyData(65, InputModifiers.Ctrl, "B");

        Assert.That(key1.Equals(key2), Is.True);
        Assert.That(key1 == key2, Is.True);
    }

    [Test]
    public void GetHashCode_WithSameValues_ShouldBeEqual()
    {
        var key1 = new InputKeyData(65, InputModifiers.Ctrl, "A");
        var key2 = new InputKeyData(65, InputModifiers.Ctrl, "A");

        Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
    }

    [Test]
    public void ToString_ShouldIncludeModifiersAndName()
    {
        var keyData = new InputKeyData(65, InputModifiers.Shift | InputModifiers.Ctrl, "A");
        var result = keyData.ToString();

        Assert.That(result, Contains.Substring("Shift"));
        Assert.That(result, Contains.Substring("Ctrl"));
        Assert.That(result, Contains.Substring("A"));
    }

    [Test]
    public void ToString_WithoutModifiers_ShouldReturnName()
    {
        var keyData = new InputKeyData(65, "A");
        var result = keyData.ToString();

        Assert.That(result, Is.EqualTo("A"));
    }
}
