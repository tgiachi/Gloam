using Gloam.Runtime.Config;

namespace Gloam.Tests.Runtime;

public class RuntimeConfigTests
{
    [Test]
    public void Constructor_WithDefaults_ShouldHaveCorrectValues()
    {
        var config = new RuntimeConfig();

        Assert.That(config.SimulationStep, Is.EqualTo(0));
        Assert.That(config.RenderStep, Is.EqualTo(15));
        Assert.That(config.AllowFrameSkip, Is.True);
    }

    [Test]
    public void Constructor_WithCustomValues_ShouldSetCorrectly()
    {
        var config = new RuntimeConfig(
            50,
            16,
            false
        );

        Assert.That(config.SimulationStep, Is.EqualTo(50));
        Assert.That(config.RenderStep, Is.EqualTo(16));
        Assert.That(config.AllowFrameSkip, Is.False);
    }

    [Test]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        var config1 = new RuntimeConfig(50, 16, false);
        var config2 = new RuntimeConfig(50, 16, false);

        Assert.That(config1, Is.EqualTo(config2));
        Assert.That(config1.GetHashCode(), Is.EqualTo(config2.GetHashCode()));
    }

    [Test]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        var config1 = new RuntimeConfig(50, 16, false);
        var config2 = new RuntimeConfig(25, 16, false);

        Assert.That(config1, Is.Not.EqualTo(config2));
    }

    [Test]
    public void ToString_ShouldContainAllProperties()
    {
        var config = new RuntimeConfig(50, 16, false);
        var result = config.ToString();

        Assert.That(result, Contains.Substring("50"));
        Assert.That(result, Contains.Substring("16"));
        Assert.That(result, Contains.Substring("False"));
    }

    [Test]
    public void TurnBasedConfig_ShouldHaveZeroSimulationStep()
    {
        var turnBasedConfig = new RuntimeConfig();

        Assert.That(turnBasedConfig.SimulationStep, Is.EqualTo(0));
    }

    [Test]
    public void RealTimeConfig_ShouldHavePositiveSimulationStep()
    {
        var realTimeConfig = new RuntimeConfig(50);

        Assert.That(realTimeConfig.SimulationStep, Is.GreaterThan(0));
    }

    [Test]
    public void HighFrameRateConfig_ShouldHaveLowRenderStep()
    {
        var highFpsConfig = new RuntimeConfig(RenderStep: 16); // ~60 FPS

        Assert.That(highFpsConfig.RenderStep, Is.EqualTo(16));
    }

    [Test]
    public void LowFrameRateConfig_ShouldHaveHighRenderStep()
    {
        var lowFpsConfig = new RuntimeConfig(RenderStep: 100); // ~10 FPS

        Assert.That(lowFpsConfig.RenderStep, Is.EqualTo(100));
    }

    [Test]
    public void WithExpression_ShouldCreateNewInstanceWithChangedValue()
    {
        var original = new RuntimeConfig(50, 16);
        var modified = original with { SimulationStep = 100 };

        Assert.That(modified.SimulationStep, Is.EqualTo(100));
        Assert.That(modified.RenderStep, Is.EqualTo(16));
        Assert.That(modified.AllowFrameSkip, Is.True);
        Assert.That(original.SimulationStep, Is.EqualTo(50)); // Original unchanged
    }
}
