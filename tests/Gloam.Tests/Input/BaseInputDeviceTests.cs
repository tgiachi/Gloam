using Gloam.Core.Input;
using Gloam.Core.Input.Base;

namespace Gloam.Tests.Input;

public class TestInputDevice : BaseInputDevice
{
    private readonly HashSet<InputKeyData> _currentlyPressed = new();

    public void SetKeyPressed(InputKeyData key, bool pressed)
    {
        if (pressed)
        {
            _currentlyPressed.Add(key);
        }
        else
        {
            _currentlyPressed.Remove(key);
        }
    }

    protected override bool GetCurrentKeyState(InputKeyData key)
    {
        return _currentlyPressed.Contains(key);
    }
}

public class BaseInputDeviceTests
{
    private TestInputDevice _inputDevice = null!;

    [SetUp]
    public void SetUp()
    {
        _inputDevice = new TestInputDevice();
    }

    [Test]
    public void IsDown_WithPressedKey_ShouldReturnTrue()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);

        Assert.That(_inputDevice.IsDown(Keys.A), Is.True);
    }

    [Test]
    public void IsDown_WithReleasedKey_ShouldReturnFalse()
    {
        _inputDevice.SetKeyPressed(Keys.A, false);

        Assert.That(_inputDevice.IsDown(Keys.A), Is.False);
    }

    [Test]
    public void WasPressed_OnFirstFrame_ShouldReturnTrue()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);

        Assert.That(_inputDevice.WasPressed(Keys.A), Is.True);
    }

    [Test]
    public void WasPressed_OnSecondFrame_ShouldReturnFalse()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);
        _inputDevice.WasPressed(Keys.A); // First check
        _inputDevice.EndFrame();

        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
    }

    [Test]
    public void WasPressed_WithUnpressedKey_ShouldReturnFalse()
    {
        _inputDevice.SetKeyPressed(Keys.A, false);

        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
    }

    [Test]
    public void WasReleased_WhenKeyReleased_ShouldReturnTrue()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);
        _inputDevice.IsDown(Keys.A); // Register as pressed
        _inputDevice.EndFrame();

        _inputDevice.SetKeyPressed(Keys.A, false);

        Assert.That(_inputDevice.WasReleased(Keys.A), Is.True);
    }

    [Test]
    public void WasReleased_WhenKeyStillPressed_ShouldReturnFalse()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);
        _inputDevice.IsDown(Keys.A);
        _inputDevice.EndFrame();

        Assert.That(_inputDevice.WasReleased(Keys.A), Is.False);
    }

    [Test]
    public void WasReleased_WhenKeyNeverPressed_ShouldReturnFalse()
    {
        _inputDevice.SetKeyPressed(Keys.A, false);

        Assert.That(_inputDevice.WasReleased(Keys.A), Is.False);
    }

    [Test]
    public void EdgeDetection_FullCycle_ShouldWorkCorrectly()
    {
        // Frame 1: Key not pressed
        Assert.That(_inputDevice.IsDown(Keys.A), Is.False);
        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
        Assert.That(_inputDevice.WasReleased(Keys.A), Is.False);
        _inputDevice.EndFrame();

        // Frame 2: Key pressed
        _inputDevice.SetKeyPressed(Keys.A, true);
        Assert.That(_inputDevice.IsDown(Keys.A), Is.True);
        Assert.That(_inputDevice.WasPressed(Keys.A), Is.True);
        Assert.That(_inputDevice.WasReleased(Keys.A), Is.False);
        _inputDevice.EndFrame();

        // Frame 3: Key held
        Assert.That(_inputDevice.IsDown(Keys.A), Is.True);
        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
        Assert.That(_inputDevice.WasReleased(Keys.A), Is.False);
        _inputDevice.EndFrame();

        // Frame 4: Key released
        _inputDevice.SetKeyPressed(Keys.A, false);
        Assert.That(_inputDevice.IsDown(Keys.A), Is.False);
        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
        Assert.That(_inputDevice.WasReleased(Keys.A), Is.True);
    }

    [Test]
    public void MultipleKeys_ShouldTrackIndependently()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);
        _inputDevice.SetKeyPressed(Keys.B, false);

        Assert.That(_inputDevice.IsDown(Keys.A), Is.True);
        Assert.That(_inputDevice.IsDown(Keys.B), Is.False);
        Assert.That(_inputDevice.WasPressed(Keys.A), Is.True);
        Assert.That(_inputDevice.WasPressed(Keys.B), Is.False);

        _inputDevice.EndFrame();

        _inputDevice.SetKeyPressed(Keys.B, true);

        Assert.That(_inputDevice.WasPressed(Keys.A), Is.False);
        Assert.That(_inputDevice.WasPressed(Keys.B), Is.True);
    }

    [Test]
    public void Poll_ShouldBeVirtualAndCallable()
    {
        Assert.DoesNotThrow(() => _inputDevice.Poll());
    }

    [Test]
    public void EndFrame_ShouldBeCallableMultipleTimes()
    {
        _inputDevice.SetKeyPressed(Keys.A, true);
        _inputDevice.IsDown(Keys.A);

        Assert.DoesNotThrow(() => _inputDevice.EndFrame());
        Assert.DoesNotThrow(() => _inputDevice.EndFrame());
    }
}
