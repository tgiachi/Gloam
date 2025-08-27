using Gloam.Core.Input;

namespace Gloam.Tests.Input;

public class InputModifiersTests
{
    [Test]
    public void None_ShouldHaveZeroValue()
    {
        Assert.That((int)InputModifiers.None, Is.EqualTo(0));
    }

    [Test]
    public void Shift_ShouldHaveUniqueValue()
    {
        Assert.That((int)InputModifiers.Shift, Is.EqualTo(1));
    }

    [Test]
    public void Ctrl_ShouldHaveUniqueValue()
    {
        Assert.That((int)InputModifiers.Ctrl, Is.EqualTo(2));
    }

    [Test]
    public void Alt_ShouldHaveUniqueValue()
    {
        Assert.That((int)InputModifiers.Alt, Is.EqualTo(4));
    }

    [Test]
    public void Meta_ShouldHaveUniqueValue()
    {
        Assert.That((int)InputModifiers.Meta, Is.EqualTo(8));
    }

    [Test]
    public void FlagsCombination_ShouldCombineCorrectly()
    {
        var combined = InputModifiers.Shift | InputModifiers.Ctrl;

        Assert.That(combined.HasFlag(InputModifiers.Shift), Is.True);
        Assert.That(combined.HasFlag(InputModifiers.Ctrl), Is.True);
        Assert.That(combined.HasFlag(InputModifiers.Alt), Is.False);
    }

    [Test]
    public void FlagsCombination_AllModifiers_ShouldWork()
    {
        var allModifiers = InputModifiers.Shift | InputModifiers.Ctrl | InputModifiers.Alt | InputModifiers.Meta;

        Assert.That(allModifiers.HasFlag(InputModifiers.Shift), Is.True);
        Assert.That(allModifiers.HasFlag(InputModifiers.Ctrl), Is.True);
        Assert.That(allModifiers.HasFlag(InputModifiers.Alt), Is.True);
        Assert.That(allModifiers.HasFlag(InputModifiers.Meta), Is.True);
    }

    [Test]
    public void FlagsEnum_ShouldSupportBitwiseOperations()
    {
        var modifier1 = InputModifiers.Shift;
        var modifier2 = InputModifiers.Ctrl;
        var combined = modifier1 | modifier2;

        Assert.That((int)combined, Is.EqualTo(3)); // 1 | 2 = 3
    }
}
