using Gloam.Runtime.Config;

namespace Gloam.Tests.Config;

/// <summary>
///     Tests for GameLoopConfig record functionality and behavior.
/// </summary>
public class GameLoopConfigTests
{
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
        Assert.That(config.SimulationStep, Is.EqualTo(TimeSpan.Zero));
        Assert.That(config.RenderStep, Is.EqualTo(TimeSpan.FromMilliseconds(33)));
        Assert.That(config.MaxSleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
    }

    [Test]
    public void Constructor_WithAllProperties_ShouldCreateInstance()
    {
        var keepRunning = () => false;
        var simulationStep = TimeSpan.FromMilliseconds(16);
        var renderStep = TimeSpan.FromMilliseconds(16);
        var maxSleepTime = TimeSpan.FromMilliseconds(10);

        var config = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SimulationStep = simulationStep,
            RenderStep = renderStep,
            MaxSleepTime = maxSleepTime
        };

        Assert.That(config.KeepRunning, Is.SameAs(keepRunning));
        Assert.That(config.SimulationStep, Is.EqualTo(simulationStep));
        Assert.That(config.RenderStep, Is.EqualTo(renderStep));
        Assert.That(config.MaxSleepTime, Is.EqualTo(maxSleepTime));
    }

    #endregion

    #region Default Values Tests

    [Test]
    public void DefaultValues_SimulationStep_ShouldBeZero()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        Assert.That(config.SimulationStep, Is.EqualTo(TimeSpan.Zero));
    }

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
    public void DefaultValues_MaxSleepTime_ShouldBe5Milliseconds()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true
        };

        Assert.That(config.MaxSleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var keepRunning = () => true;
        var simulationStep = TimeSpan.FromMilliseconds(16);
        var renderStep = TimeSpan.FromMilliseconds(20);
        var maxSleepTime = TimeSpan.FromMilliseconds(8);

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SimulationStep = simulationStep,
            RenderStep = renderStep,
            MaxSleepTime = maxSleepTime
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SimulationStep = simulationStep,
            RenderStep = renderStep,
            MaxSleepTime = maxSleepTime
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
            KeepRunning = () => true  // Different function instance
        };

        Assert.That(config1, Is.Not.EqualTo(config2));
    }

    [Test]
    public void Equality_DifferentSimulationStep_ShouldNotBeEqual()
    {
        var keepRunning = () => true;

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SimulationStep = TimeSpan.FromMilliseconds(16)
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            SimulationStep = TimeSpan.FromMilliseconds(20)
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
    public void Equality_DifferentMaxSleepTime_ShouldNotBeEqual()
    {
        var keepRunning = () => true;

        var config1 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            MaxSleepTime = TimeSpan.FromMilliseconds(5)
        };

        var config2 = new GameLoopConfig
        {
            KeepRunning = keepRunning,
            MaxSleepTime = TimeSpan.FromMilliseconds(10)
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
        Assert.That(modified.SimulationStep, Is.EqualTo(original.SimulationStep));
        Assert.That(modified.RenderStep, Is.EqualTo(original.RenderStep));
        Assert.That(modified.MaxSleepTime, Is.EqualTo(original.MaxSleepTime));
    }

    [Test]
    public void With_ChangeSimulationStep_ShouldCreateNewInstance()
    {
        var original = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.FromMilliseconds(16)
        };

        var newSimulationStep = TimeSpan.FromMilliseconds(20);
        var modified = original with { SimulationStep = newSimulationStep };

        Assert.That(original.SimulationStep, Is.EqualTo(TimeSpan.FromMilliseconds(16)));
        Assert.That(modified.SimulationStep, Is.EqualTo(newSimulationStep));
        Assert.That(modified.KeepRunning, Is.SameAs(original.KeepRunning));
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
    public void With_ChangeMaxSleepTime_ShouldCreateNewInstance()
    {
        var original = new GameLoopConfig
        {
            KeepRunning = () => true,
            MaxSleepTime = TimeSpan.FromMilliseconds(5)
        };

        var newMaxSleepTime = TimeSpan.FromMilliseconds(10);
        var modified = original with { MaxSleepTime = newMaxSleepTime };

        Assert.That(original.MaxSleepTime, Is.EqualTo(TimeSpan.FromMilliseconds(5)));
        Assert.That(modified.MaxSleepTime, Is.EqualTo(newMaxSleepTime));
        Assert.That(modified.KeepRunning, Is.SameAs(original.KeepRunning));
    }

    #endregion

    #region Configuration Scenario Tests

    [Test]
    public void Configuration_RealTimeMode_ShouldHaveFixedSimulationStep()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.FromMilliseconds(16), // 60 FPS
            RenderStep = TimeSpan.FromMilliseconds(16)       // 60 FPS
        };

        Assert.That(config.SimulationStep, Is.GreaterThan(TimeSpan.Zero));
        Assert.That(config.RenderStep, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public void Configuration_TurnBasedMode_ShouldHaveZeroSimulationStep()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.Zero,
            RenderStep = TimeSpan.FromMilliseconds(100) // Slower refresh for turn-based
        };

        Assert.That(config.SimulationStep, Is.EqualTo(TimeSpan.Zero));
        Assert.That(config.RenderStep, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public void Configuration_HighPerformanceMode_ShouldHaveShortSteps()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.FromMilliseconds(8),  // 120 FPS
            RenderStep = TimeSpan.FromMilliseconds(8),       // 120 FPS
            MaxSleepTime = TimeSpan.FromMilliseconds(1)      // Minimal sleep
        };

        Assert.That(config.SimulationStep.TotalMilliseconds, Is.LessThan(10));
        Assert.That(config.RenderStep.TotalMilliseconds, Is.LessThan(10));
        Assert.That(config.MaxSleepTime.TotalMilliseconds, Is.LessThan(5));
    }

    [Test]
    public void Configuration_LowPerformanceMode_ShouldHaveLongerSteps()
    {
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.FromMilliseconds(50), // 20 FPS
            RenderStep = TimeSpan.FromMilliseconds(50),      // 20 FPS
            MaxSleepTime = TimeSpan.FromMilliseconds(20)     // Longer sleep for battery saving
        };

        Assert.That(config.SimulationStep.TotalMilliseconds, Is.GreaterThan(30));
        Assert.That(config.RenderStep.TotalMilliseconds, Is.GreaterThan(30));
        Assert.That(config.MaxSleepTime.TotalMilliseconds, Is.GreaterThan(10));
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public void EdgeCase_NegativeSimulationStep_ShouldBeAllowed()
    {
        // Note: While unusual, negative TimeSpan values are technically valid
        // The implementation should handle this gracefully
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = TimeSpan.FromMilliseconds(-10)
        };

        Assert.That(config.SimulationStep, Is.EqualTo(TimeSpan.FromMilliseconds(-10)));
    }

    [Test]
    public void EdgeCase_VeryLargeTimeSpan_ShouldBeAllowed()
    {
        var largeTimeSpan = TimeSpan.FromHours(1);
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            SimulationStep = largeTimeSpan,
            RenderStep = largeTimeSpan,
            MaxSleepTime = largeTimeSpan
        };

        Assert.That(config.SimulationStep, Is.EqualTo(largeTimeSpan));
        Assert.That(config.RenderStep, Is.EqualTo(largeTimeSpan));
        Assert.That(config.MaxSleepTime, Is.EqualTo(largeTimeSpan));
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

    #region Performance Tests

    [Test]
    public void Performance_RecordCreation_ShouldBeEfficient()
    {
        var keepRunning = () => true;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Create many instances
        for (var i = 0; i < 10000; i++)
        {
            var config = new GameLoopConfig
            {
                KeepRunning = keepRunning,
                SimulationStep = TimeSpan.FromMilliseconds(i % 100),
                RenderStep = TimeSpan.FromMilliseconds(33),
                MaxSleepTime = TimeSpan.FromMilliseconds(5)
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

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Create many modified instances
        GameLoopConfig? current = original;
        for (var i = 0; i < 1000; i++)
        {
            current = current with { SimulationStep = TimeSpan.FromMilliseconds(i) };
        }

        stopwatch.Stop();

        // Should create 1k modified records in reasonable time (less than 50ms)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50));
        Assert.That(current.SimulationStep, Is.EqualTo(TimeSpan.FromMilliseconds(999)));
    }

    #endregion
}