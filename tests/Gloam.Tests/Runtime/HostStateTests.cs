using Gloam.Runtime.Types;

namespace Gloam.Tests.Runtime;

public class HostStateTests
{
    [Test]
    public void HostState_ShouldHaveAllExpectedValues()
    {
        var expectedStates = new[]
        {
            HostState.Created,
            HostState.Initialized,
            HostState.ContentLoaded,
            HostState.SessionCreated,
            HostState.Running,
            HostState.Paused,
            HostState.Stopped,
            HostState.Disposed
        };

        var actualStates = Enum.GetValues<HostState>();

        Assert.That(actualStates, Is.EquivalentTo(expectedStates));
    }

    [Test]
    public void HostState_ShouldHaveCorrectValues()
    {
        Assert.That((int)HostState.Created, Is.EqualTo(0));
        Assert.That((int)HostState.Initialized, Is.EqualTo(1));
        Assert.That((int)HostState.ContentLoaded, Is.EqualTo(2));
        Assert.That((int)HostState.SessionCreated, Is.EqualTo(3));
        Assert.That((int)HostState.Running, Is.EqualTo(4));
        Assert.That((int)HostState.Paused, Is.EqualTo(5));
        Assert.That((int)HostState.Stopped, Is.EqualTo(6));
        Assert.That((int)HostState.Disposed, Is.EqualTo(7));
    }

    [Test]
    public void HostState_ShouldBeConvertibleToString()
    {
        Assert.That(HostState.Created.ToString(), Is.EqualTo("Created"));
        Assert.That(HostState.Running.ToString(), Is.EqualTo("Running"));
        Assert.That(HostState.Disposed.ToString(), Is.EqualTo("Disposed"));
    }

    [Test]
    public void HostState_ShouldBeComparable()
    {
        Assert.That(HostState.Created, Is.LessThan(HostState.Initialized));
        Assert.That(HostState.Running, Is.GreaterThan(HostState.Created));
        Assert.That(HostState.Disposed, Is.GreaterThan(HostState.Stopped));
    }

    [Test]
    public void HostState_ShouldHaveLogicalProgression()
    {
        // Test that states have logical ordering for typical lifecycle
        Assert.That(HostState.Created, Is.LessThan(HostState.Initialized));
        Assert.That(HostState.Initialized, Is.LessThan(HostState.ContentLoaded));
        Assert.That(HostState.ContentLoaded, Is.LessThan(HostState.SessionCreated));
        Assert.That(HostState.SessionCreated, Is.LessThan(HostState.Running));
        Assert.That(HostState.Running, Is.LessThan(HostState.Stopped));
        Assert.That(HostState.Stopped, Is.LessThan(HostState.Disposed));
    }

    [Test]
    public void HostState_PausedShouldBeValid()
    {
        // Paused can occur during running state, so verify it exists
        Assert.That(HostState.Paused, Is.Not.EqualTo(default(HostState)));
        Assert.That((int)HostState.Paused, Is.EqualTo(5));
    }

    [TestCase(HostState.Created)]
    [TestCase(HostState.Initialized)]
    [TestCase(HostState.ContentLoaded)]
    [TestCase(HostState.SessionCreated)]
    [TestCase(HostState.Running)]
    [TestCase(HostState.Paused)]
    [TestCase(HostState.Stopped)]
    [TestCase(HostState.Disposed)]
    public void HostState_AllValues_ShouldBeValid(HostState state)
    {
        Assert.That(state, Is.Not.EqualTo(default(HostState)).Or.EqualTo(HostState.Created));
        Assert.That(state.ToString(), Is.Not.Empty);
    }
}
