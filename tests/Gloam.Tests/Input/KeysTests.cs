using System.Reflection;
using Gloam.Core.Input;

namespace Gloam.Tests.Input;

public class KeysTests
{
    [Test]
    public void ControlKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.None.KeyCode, Is.EqualTo(0));
        Assert.That(Keys.Backspace.KeyCode, Is.EqualTo(8));
        Assert.That(Keys.Tab.KeyCode, Is.EqualTo(9));
        Assert.That(Keys.Enter.KeyCode, Is.EqualTo(13));
        Assert.That(Keys.Escape.KeyCode, Is.EqualTo(27));
        Assert.That(Keys.Space.KeyCode, Is.EqualTo(32));
    }

    [Test]
    public void NavigationKeysShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.Left.KeyCode, Is.EqualTo(37));
        Assert.That(Keys.Up.KeyCode, Is.EqualTo(38));
        Assert.That(Keys.Right.KeyCode, Is.EqualTo(39));
        Assert.That(Keys.Down.KeyCode, Is.EqualTo(40));
        Assert.That(Keys.Home.KeyCode, Is.EqualTo(36));
        Assert.That(Keys.End.KeyCode, Is.EqualTo(35));
    }

    [Test]
    public void NumberKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.D0.KeyCode, Is.EqualTo(48));
        Assert.That(Keys.D1.KeyCode, Is.EqualTo(49));
        Assert.That(Keys.D5.KeyCode, Is.EqualTo(53));
        Assert.That(Keys.D9.KeyCode, Is.EqualTo(57));
    }

    [Test]
    public void LetterKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.A.KeyCode, Is.EqualTo(65));
        Assert.That(Keys.B.KeyCode, Is.EqualTo(66));
        Assert.That(Keys.Z.KeyCode, Is.EqualTo(90));
    }

    [Test]
    public void FunctionKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.F1.KeyCode, Is.EqualTo(112));
        Assert.That(Keys.F12.KeyCode, Is.EqualTo(123));
        Assert.That(Keys.F24.KeyCode, Is.EqualTo(135));
    }

    [Test]
    public void ModifierKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.LeftShift.KeyCode, Is.EqualTo(160));
        Assert.That(Keys.RightShift.KeyCode, Is.EqualTo(161));
        Assert.That(Keys.LeftCtrl.KeyCode, Is.EqualTo(162));
        Assert.That(Keys.RightCtrl.KeyCode, Is.EqualTo(163));
    }

    [Test]
    public void NumPadKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.NumPad0.KeyCode, Is.EqualTo(96));
        Assert.That(Keys.NumPad9.KeyCode, Is.EqualTo(105));
        Assert.That(Keys.Add.KeyCode, Is.EqualTo(107));
        Assert.That(Keys.Subtract.KeyCode, Is.EqualTo(109));
    }

    [Test]
    public void PunctuationKeys_ShouldHaveCorrectKeyCodes()
    {
        Assert.That(Keys.Semicolon.KeyCode, Is.EqualTo(186));
        Assert.That(Keys.Comma.KeyCode, Is.EqualTo(188));
        Assert.That(Keys.Period.KeyCode, Is.EqualTo(190));
        Assert.That(Keys.Slash.KeyCode, Is.EqualTo(191));
    }

    [Test]
    public void Aliases_ShouldReferenceCorrectKeys()
    {
        Assert.That(Keys.UpArrow, Is.EqualTo(Keys.Up));
        Assert.That(Keys.DownArrow, Is.EqualTo(Keys.Down));
        Assert.That(Keys.LeftArrow, Is.EqualTo(Keys.Left));
        Assert.That(Keys.RightArrow, Is.EqualTo(Keys.Right));

        Assert.That(Keys.Esc, Is.EqualTo(Keys.Escape));
        Assert.That(Keys.Ctrl, Is.EqualTo(Keys.LeftCtrl));
        Assert.That(Keys.Shift, Is.EqualTo(Keys.LeftShift));
        Assert.That(Keys.Alt, Is.EqualTo(Keys.LeftAlt));
    }

    [Test]
    public void RoguelikeAliases_ShouldReferenceCorrectKeys()
    {
        Assert.That(Keys.Question, Is.EqualTo(Keys.Slash));
        Assert.That(Keys.LessThan, Is.EqualTo(Keys.Comma));
        Assert.That(Keys.GreaterThan, Is.EqualTo(Keys.Period));
        Assert.That(Keys.Colon, Is.EqualTo(Keys.Semicolon));
    }

    [Test]
    public void KeyNames_ShouldBeDescriptive()
    {
        Assert.That(Keys.A.Name, Is.EqualTo("A"));
        Assert.That(Keys.Enter.Name, Is.EqualTo("Enter"));
        Assert.That(Keys.F1.Name, Is.EqualTo("F1"));
        Assert.That(Keys.NumPad5.Name, Is.EqualTo("NumPad5"));
    }

    [Test]
    public void AllKeys_ShouldHaveValidKeyCodes()
    {
        var keyProperties = typeof(Keys).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(InputKeyData));

        foreach (var property in keyProperties)
        {
            var key = (InputKeyData)property.GetValue(null)!;
            Assert.That(key.KeyCode, Is.GreaterThanOrEqualTo(0), $"Key {property.Name} should have valid KeyCode");
            Assert.That(key.Name, Is.Not.Null, $"Key {property.Name} should have non-null Name");
        }
    }

    [Test]
    public void DuplicateKeys_ShouldNotExist()
    {
        // Get all key fields excluding known aliases
        var aliasNames = new HashSet<string>
        {
            "UpArrow", "DownArrow", "LeftArrow", "RightArrow",
            "Esc", "Ctrl", "Shift", "Alt", "Win", "Menu",
            "Question", "LessThan", "GreaterThan", "Colon",
            "Return", "Capital" // Additional aliases
        };

        var keyProperties = typeof(Keys).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(InputKeyData))
            .Where(f => !aliasNames.Contains(f.Name))
            .ToList();

        var keyCodes = keyProperties.Select(p => ((InputKeyData)p.GetValue(null)!).KeyCode).ToList();
        var uniqueKeyCodes = keyCodes.Distinct().ToList();

        if (keyCodes.Count != uniqueKeyCodes.Count)
        {
            var duplicates = keyCodes.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            Assert.Fail($"Duplicate key codes found: {string.Join(", ", duplicates)}");
        }

        Assert.That(
            keyCodes.Count,
            Is.EqualTo(uniqueKeyCodes.Count),
            "No duplicate key codes should exist (excluding aliases)"
        );
    }
}
