using System.Diagnostics;
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;

namespace Gloam.Tests.Runtime;

/// <summary>
/// Tests for the new loop management functionality in GloamHost
/// </summary>
[TestFixture]
public class GloamHostLoopTests
{
    private GloamHostConfig _hostConfig = null!;
    private GloamHost _host = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        // Create a unique temporary directory for each test
        _tempDirectory = Path.Combine(Path.GetTempPath(), "GloamLoopTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _hostConfig = new GloamHostConfig
        {
            RootDirectory = _tempDirectory,
            LoaderType = ContentLoaderType.FileSystem
        };

        _host = new GloamHost(_hostConfig);
    }

    [TearDown]
    public void TearDown()
    {
        _host?.Dispose();

        // Clean up temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public async Task RunAsync_InternalMode_ExecutesLoopInternally()
    {
        // Arrange
        var loopCount = 0;

        var config = new GameLoopConfig
        {
            KeepRunning = () => loopCount++ < 5, // Run 5 iterations
            LoopMode = LoopMode.Internal,
            TargetFps = 1000.0 / 1, // 1000 FPS - Fast rendering
            SleepTime = TimeSpan.Zero // No sleep for testing
        };

        // Act
        await _host.RunAsync(config);

        // Assert
        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(loopCount, Is.GreaterThanOrEqualTo(6)); // Should have executed at least 6 times (5 true + 1 false)
    }

    [Test]
    public async Task RunAsync_ExternalMode_DoesNotExecuteLoop()
    {
        // Arrange
        var loopExecuted = false;

        var config = new GameLoopConfig
        {
            KeepRunning = () => { loopExecuted = true; return false; }, // Should not be called
            LoopMode = LoopMode.External
        };

        // Act
        await _host.RunAsync(config);

        // Assert
        Assert.That(_host.State, Is.EqualTo(HostState.Running)); // Should remain running for external mode
        Assert.That(loopExecuted, Is.False); // KeepRunning should not be called in external mode
    }

    [Test]
    public async Task LoopAsync_ExternalMode_ExecutesSingleIteration()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            HandleTimingInExternalMode = false // No automatic timing
        };

        await _host.StartAsync();

        // Act
        await _host.LoopAsync(config);

        // Assert
        Assert.That(_host.State, Is.EqualTo(HostState.Running));
    }

    [Test]
    public void LoopAsync_InternalMode_ThrowsException()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.Internal // Wrong mode for LoopAsync
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _host.LoopAsync(config));
    }

    [Test]
    public void LoopAsync_WhenHostNotRunning_ThrowsException()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _host.LoopAsync(config));
    }

    [Test]
    public void GetLoopState_ReturnsCorrectState()
    {
        // Arrange & Act
        var (isFirstFrame, timeSinceStart, timeSinceLastRender) = _host.GetLoopState();

        // Assert
        Assert.That(isFirstFrame, Is.True);
        Assert.That(timeSinceStart, Is.EqualTo(TimeSpan.Zero));
        Assert.That(timeSinceLastRender, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void ResetLoopState_ResetsAllState()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External
        };

        // Act
        _host.ResetLoopState();
        var (isFirstFrame, timeSinceStart, timeSinceLastRender) = _host.GetLoopState();

        // Assert
        Assert.That(isFirstFrame, Is.True);
        Assert.That(timeSinceStart, Is.EqualTo(TimeSpan.Zero));
        Assert.That(timeSinceLastRender, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public async Task LoopAsync_WithTimingEnabled_HandlesTimingCorrectly()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            HandleTimingInExternalMode = true,
            SleepTime = TimeSpan.FromMilliseconds(10) // Short sleep for testing
        };

        await _host.StartAsync();

        // Act
        var startTime = Stopwatch.GetTimestamp();
        await _host.LoopAsync(config);
        var endTime = Stopwatch.GetTimestamp();

        // Assert
        var elapsed = Stopwatch.GetElapsedTime(startTime, endTime);
        Assert.That(elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(10))); // Should have slept
    }

    [Test]
    public async Task LoopAsync_WithTimingDisabled_DoesNotHandleTiming()
    {
        // Arrange
        var config = new GameLoopConfig
        {
            KeepRunning = () => true,
            LoopMode = LoopMode.External,
            HandleTimingInExternalMode = false, // No timing handling
            SleepTime = TimeSpan.FromMilliseconds(100) // Long sleep that should be ignored
        };

        await _host.StartAsync();

        // Act
        var startTime = Stopwatch.GetTimestamp();
        await _host.LoopAsync(config);
        var endTime = Stopwatch.GetTimestamp();

        // Assert
        var elapsed = Stopwatch.GetElapsedTime(startTime, endTime);
        Assert.That(elapsed, Is.LessThan(TimeSpan.FromMilliseconds(50))); // Should complete quickly
    }
}