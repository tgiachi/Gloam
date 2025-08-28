using System.Diagnostics;
using Gloam.Core.Contexts;
using Gloam.Core.Utils;

namespace Gloam.Tests.Utils;

public class FrameInfoUtilsTests
{
    [Test]
    public void Create_WithValidParameters_ShouldReturnFrameInfo()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo, Is.Not.Null);
        Assert.That(frameInfo.DeltaTime, Is.EqualTo(timeSinceLastRender));
        Assert.That(frameInfo.FramesPerSecond, Is.GreaterThan(0));
    }

    [Test]
    public void Create_WithFirstFrame_ShouldSetFrameNumberToZero()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = true;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FrameNumber, Is.EqualTo(0));
        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(0f)); // First frame has 0 FPS
    }

    [Test]
    public void Create_WithFirstFrame_ShouldSetFramesPerSecondToZero()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = true;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(0f));
    }

    [Test]
    public void Create_WithNonFirstFrame_ShouldCalculateFrameNumber()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FrameNumber, Is.GreaterThan(0));
    }

    [Test]
    public void Create_WithNonFirstFrame_ShouldCalculateFramesPerSecond()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67); // ~60 FPS
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        // Should be approximately 60 FPS (allowing for some precision variance)
        Assert.That(frameInfo.FramesPerSecond, Is.InRange(59f, 61f));
    }

    [Test]
    public void Create_WithZeroTimeSinceLastRender_ShouldSetFramesPerSecondToZero()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.Zero;
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(0f));
    }

    [Test]
    public void Create_WithNegativeTimeSinceLastRender_ShouldSetFramesPerSecondToZero()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(-10);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(0f));
    }

    [Test]
    public void Create_ShouldSetTotalTimeBasedOnTimestamp()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.TotalTime, Is.GreaterThan(TimeSpan.Zero));
        Assert.That(frameInfo.TotalTime.TotalMilliseconds, Is.GreaterThan(0));
    }

    [Test]
    public void Create_WithDifferentRenderSteps_ShouldCalculateCorrectFrameNumber()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;

        var renderStep1 = TimeSpan.FromMilliseconds(16.67); // 60 FPS
        var renderStep2 = TimeSpan.FromMilliseconds(33.33); // 30 FPS

        var startTimestamp = 0L;
        var frameInfo1 = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep1);
        var frameInfo2 = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep2);

        // Same timestamp, different render steps should yield different frame numbers
        Assert.That(frameInfo1.FrameNumber, Is.Not.EqualTo(frameInfo2.FrameNumber));
        Assert.That(frameInfo1.FrameNumber, Is.GreaterThan(frameInfo2.FrameNumber));
    }

    [Test]
    public void Create_WithVerySmallTimeSinceLastRender_ShouldCalculateHighFPS()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(1); // 1ms = 1000 FPS
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(1000f));
    }

    [Test]
    public void Create_WithLargeTimeSinceLastRender_ShouldCalculateLowFPS()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(100); // 100ms = 10 FPS
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.FramesPerSecond, Is.EqualTo(10f));
    }

    [Test]
    public void Create_ShouldPreserveDeltaTimeExactly()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(23.456);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo.DeltaTime, Is.EqualTo(timeSinceLastRender));
        Assert.That(frameInfo.DeltaTime.TotalMilliseconds, Is.EqualTo(23.456));
    }

    [Test]
    public void Create_WithSequentialCalls_ShouldProduceConsistentResults()
    {
        var baseTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var frameInfo1 = FrameInfoUtils.Create(startTimestamp, baseTimestamp, timeSinceLastRender, isFirstFrame, renderStep);
        var frameInfo2 = FrameInfoUtils.Create(startTimestamp, baseTimestamp, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo1.FrameNumber, Is.EqualTo(frameInfo2.FrameNumber));
        Assert.That(frameInfo1.FramesPerSecond, Is.EqualTo(frameInfo2.FramesPerSecond));
        Assert.That(frameInfo1.DeltaTime, Is.EqualTo(frameInfo2.DeltaTime));
        Assert.That(frameInfo1.TotalTime, Is.EqualTo(frameInfo2.TotalTime));
    }

    [Test]
    public void Create_WithZeroRenderStep_ShouldHandleGracefully()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.Zero;

        // Should not throw division by zero
        Assert.DoesNotThrow(() => 
        {
            var startTimestamp = 0L;
        var frameInfo = FrameInfoUtils.Create(startTimestamp, currentTimestamp, timeSinceLastRender, isFirstFrame, renderStep);
            Assert.That(frameInfo, Is.Not.Null);
        });
    }

    [Test]
    public void Create_WithDifferentTimestamps_ShouldProduceDifferentTotalTimes()
    {
        var timestamp1 = Stopwatch.GetTimestamp();
        Thread.Sleep(1); // Ensure different timestamps
        var timestamp2 = Stopwatch.GetTimestamp();
        
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = Stopwatch.GetTimestamp();
        var frameInfo1 = FrameInfoUtils.Create(startTimestamp, timestamp1, timeSinceLastRender, isFirstFrame, renderStep);
        var frameInfo2 = FrameInfoUtils.Create(startTimestamp, timestamp2, timeSinceLastRender, isFirstFrame, renderStep);

        Assert.That(frameInfo1.TotalTime, Is.LessThan(frameInfo2.TotalTime));
    }
}