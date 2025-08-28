using System.Diagnostics;
using Gloam.Runtime.Config;

namespace Gloam.Tests.Config;

/// <summary>
///     Tests for GameLoopConfig record functionality and behavior.
/// </summary>
public class GameLoopConfigTests
{
    #region ToString Tests

    [Test]
    public void ToString_ShouldContainTypeName()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        var toString = config.ToString();
        Assert.That(toString, Does.Contain("GameLoopConfig"));
    }

    #endregion

    #region Constructor and Property Tests

    [Test]
    public void Constructor_WithRequiredProperties_ShouldCreateInstance()
    {
        var keepRunning = () => true;
        var config = new GameLoopConfig
        {
            KeepRunning = keepRunning
        };

        Assert.That(config.KeepRunning, Is.SameAs(keepRunning));
        Assert.That(config.RenderStep, Is.EqualTo(TimeSpan.FromMilliseconds(33)));
        Assert.That(config.SleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
    }

    [Test]
    public void Constructor_WithAllProperties_ShouldCreateInstance()
    {
        var keepRunning = () => false;
        var renderStep = TimeSpan.FromMilliseconds(16);
        var sleepTime = TimeSpan.FromMilliseconds(10);

        var config = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            RenderStep = renderStep,
            SleepTime = sleepTime
        };

        Assert.That(config.KeepRunning, Is.SameAs(keepRunning));
        Assert.That(config.RenderStep, Is.EqualTo(renderStep));
        Assert.That(config.SleepTime, Is.EqualTo(sleepTime));
    }

    #endregion

    #region Default Values Tests

    [Test]
    public void DefaultValues_RenderStep_ShouldBe33Milliseconds()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        Assert.That(config.RenderStep, Is.EqualTo(TimeSpan.FromMilliseconds(33)));
    }

    [Test]
    public void DefaultValues_SleepTime_ShouldBe5Milliseconds()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        Assert.That(config.SleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var keepRunning = () => true;
        var renderStep = TimeSpan.FromMilliseconds(20);
        var sleepTime = TimeSpan.FromMilliseconds(8);

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            RenderStep = renderStep,
            SleepTime = sleepTime
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            RenderStep = renderStep,
            SleepTime = sleepTime
        };

        Assert.That(config1, Is.EqualTo(config2));
        Assert.That(config1.GetHashCode(), Is.EqualTo(config2.GetHashCode()));
    }

    [Test]
    public void Equality_DifferentKeepRunning_ShouldNotBeEqual()
    {
        var config1 = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = () => true // Different function instance
        };

        Assert.That(config1, Is.Not.EqualTo(config2));
    }


    [Test]
    public void Equality_DifferentRenderStep_ShouldNotBeEqual()
    {
        var keepRunning = () => true;

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            RenderStep = TimeSpan.FromMilliseconds(16)
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            RenderStep = TimeSpan.FromMilliseconds(20)
        };

        Assert.That(config1, Is.Not.EqualTo(config2));
    }

    [Test]
    public void Equality_DifferentSleepTime_ShouldNotBeEqual()
    {
        var keepRunning = () => true;

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SleepTime = TimeSpan.FromMilliseconds(5)
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SleepTime = TimeSpan.FromMilliseconds(10)
        };

        Assert.That(config1, Is.Not.EqualTo(config2));
    }

    #endregion

    #region Record Immutability Tests

    [Test]
    public void With_ChangeKeepRunning_ShouldCreateNewInstance()
    {
        var originalKeepRunning = () => true;
        var newKeepRunning = () => false;

        var original = new GameLoopConfig
        {
            KeepRunning = originalKeepRunning
        };

        var modified = original with { KeepRunning = newKeepRunning };

        Assert.That(original.KeepRunning, Is.SameAs(originalKeepRunning));
        Assert.That(modified.KeepRunning, Is.SameAs(newKeepRunning));
        Assert.That(modified.RenderStep, Is.EqualTo(original.RenderStep));
        Assert.That(modified.SleepTime, Is.EqualTo(original.SleepTime));
    }


    [Test]
    public void With_ChangeRenderStep_ShouldCreateNewInstance()
    {
        var original = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = TimeSpan.FromMilliseconds(33)
        };

        var newRenderStep = TimeSpan.FromMilliseconds(16);
        var modified = original with { RenderStep = newRenderStep };

        Assert.That(original.RenderStep, Is.EqualTo(TimeSpan.FromMilliseconds(33)));
        Assert.That(modified.RenderStep, Is.EqualTo(newRenderStep));
        Assert.That(modified.KeepRunning, Is.SameAs(original.KeepRunning));
    }

    [Test]
    public void With_ChangeSleepTime_ShouldCreateNewInstance()
    {
        var original = new GameLoopConfig
        {
            KeepRunning = () => true,
            SleepTime = TimeSpan.FromMilliseconds(5)
        };

        var newSleepTime = TimeSpan.FromMilliseconds(10);
        var modified = original with { SleepTime = newSleepTime };

        Assert.That(original.SleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
        Assert.That(modified.SleepTime, Is.EqualTo(newSleepTime));
        Assert.That(modified.KeepRunning, Is.SameAs(original.KeepRunning));
    }

    #endregion

    #region Configuration Scenario Tests

    [Test]
    public void Configuration_RoguelikeMode_ShouldHaveReasonableRenderStep()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = TimeSpan.FromMilliseconds(100) // Slower refresh for roguelike
        };

        Assert.That(config.RenderStep, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public void Configuration_HighPerformanceMode_ShouldHaveShortSteps()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = TimeSpan.FromMilliseconds(8), // 120 FPS
            SleepTime = TimeSpan.FromMilliseconds(1)   // Minimal sleep
        };

        Assert.That(config.RenderStep.TotalMilliseconds, Is.LessThan(10));
        Assert.That(config.SleepTime.TotalMilliseconds, Is.LessThan(5));
    }

    [Test]
    public void Configuration_LowPerformanceMode_ShouldHaveLongerSteps()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = TimeSpan.FromMilliseconds(50), // 20 FPS
            SleepTime = TimeSpan.FromMilliseconds(20)   // Longer sleep for battery saving
        };

        Assert.That(config.RenderStep.TotalMilliseconds, Is.GreaterThan(30));
        Assert.That(config.SleepTime.TotalMilliseconds, Is.GreaterThan(10));
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public void EdgeCase_NegativeSleepTime_ShouldBeAllowed()
    {
        // Note: While unusual, negative TimeSpan values are technically valid
        // The implementation should handle this gracefully
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SleepTime = TimeSpan.FromMilliseconds(-10)
        };

        Assert.That(config.SleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(-10)));
    }

    [Test]
    public void EdgeCase_VeryLargeTimeSpan_ShouldBeAllowed()
    {
        var largeTimeSpan = TimeSpan.FromHours(1);
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            RenderStep = largeTimeSpan,
            SleepTime = largeTimeSpan
        };

        Assert.That(config.RenderStep, Is.EqualTo(largeTimeSpan));
        Assert.That(config.SleepTime, Is.EqualTo(largeTimeSpan));
    }

    [Test]
    public void EdgeCase_KeepRunningThrowsException_ShouldNotAffectConfig()
    {
        Func<bool> throwingFunction = () => throw new InvalidOperationException("Test exception");
        var config = new GameLoopConfig
        {
            KeepRunning = throwingFunction
        };

        // Configuration creation should succeed even if the function would throw
        Assert.That(config.KeepRunning, Is.SameAs(throwingFunction));
        Assert.Throws<InvalidOperationException>(() => config.KeepRunning());
    }

    #endregion

    #region Performance Tests

    [Test]
    public void Performance_RecordCreation_ShouldBeEfficient()
    {
        var keepRunning = () => true;
        var stopwatch = Stopwatch.StartNew();

        // Create many instances
        for (var i = 0; i < 10000; i++)
        {
            var config = new GameLoopConfig
            {
                KeepRunning = keepRunning,
                RenderStep = TimeSpan.FromMilliseconds(33),
                SleepTime = TimeSpan.FromMilliseconds(5)
            };
        }

        stopwatch.Stop();

        // Should create 10k records in reasonable time (less than 100ms)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
    }

    [Test]
    public void Performance_RecordWith_ShouldBeEfficient()
    {
        var original = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        var stopwatch = Stopwatch.StartNew();

        // Create many modified instances
        var current = original;
        for (var i = 0; i < 1000; i++)
        {
            current = current with { RenderStep = TimeSpan.FromMilliseconds(i) };
        }

        stopwatch.Stop();

        // Should create 1k modified records in reasonable time (less than 50ms)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50));
        Assert.That(current.RenderStep, Is.EqualTo(TimeSpan.FromMilliseconds(999)));
    }

    #endregion
}
