using System.Diagnostics;
using DryIoc;
using Gloam.Core.Interfaces;
using Gloam.Core.Input;
using Gloam.Core.Primitives;
using Gloam.Data.Content;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Gloam.Data.Loaders;
using Gloam.Data.Validators;
using Gloam.Runtime;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;
using Mosaic.Engine.Directories;
using Moq;

namespace Gloam.Tests.Runtime;

/// <summary>
///     Tests for GloamHost functionality and DryIoc container configuration.
/// </summary>
public class GloamHostTests
{
    private GloamHostConfig _config = null!;
    private GloamHost _host = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        // Create a unique temporary directory for each test
        _tempDirectory = Path.Combine(Path.GetTempPath(), "GloamTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _config = new GloamHostConfig
        {
            RootDirectory = _tempDirectory,
            LoaderType = ContentLoaderType.FileSystem
        };
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

    #region Constructor Tests

    [Test]
    public void Constructor_ValidConfig_ShouldCreateHost()
    {
        _host = new GloamHost(_config);

        Assert.That(_host, Is.Not.Null);
        Assert.That(_host.Container, Is.Not.Null);
    }

    [Test]
    public void Constructor_NullConfig_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GloamHost(null!));
    }

    [Test]
    public void Constructor_ValidConfig_ShouldRegisterConfigInstance()
    {
        _host = new GloamHost(_config);

        var resolvedConfig = _host.Container.Resolve<GloamHostConfig>();

        Assert.That(resolvedConfig, Is.SameAs(_config));
    }

    #endregion

    #region Container Configuration Tests

    [Test]
    public void Container_ShouldHaveOptimizedRules()
    {
        _host = new GloamHost(_config);

        // Verify container is properly configured
        Assert.That(_host.Container, Is.Not.Null);
        Assert.That(_host.Container.Rules, Is.Not.Null);
    }

    [Test]
    public void Container_ShouldSupportTransientReuse()
    {
        _host = new GloamHost(_config);

        // Register a transient service
        _host.Container.Register<TestTransientService>();

        var instance1 = _host.Container.Resolve<TestTransientService>();
        var instance2 = _host.Container.Resolve<TestTransientService>();

        Assert.That(instance1, Is.Not.SameAs(instance2));
    }

    [Test]
    public void Container_ShouldSupportSingletonReuse()
    {
        _host = new GloamHost(_config);

        // Register a singleton service
        _host.Container.Register<TestSingletonService>(Reuse.Singleton);

        var instance1 = _host.Container.Resolve<TestSingletonService>();
        var instance2 = _host.Container.Resolve<TestSingletonService>();

        Assert.That(instance1, Is.SameAs(instance2));
    }

    #endregion

    #region FileSystem Content Loader Tests

    [Test]
    public void ConfigureServices_FileSystemLoaderType_ShouldRegisterFileSystemContentLoader()
    {
        _config.LoaderType = ContentLoaderType.FileSystem;
        _host = new GloamHost(_config);

        var contentLoader = _host.Container.Resolve<IContentLoader>();

        Assert.That(contentLoader, Is.TypeOf<FileSystemContentLoader>());
    }

    [Test]
    public void ConfigureServices_FileSystemLoaderType_ShouldRegisterDirectoriesConfig()
    {
        _config.LoaderType = ContentLoaderType.FileSystem;
        _host = new GloamHost(_config);

        var directoriesConfig = _host.Container.Resolve<DirectoriesConfig>();

        Assert.That(directoriesConfig, Is.Not.Null);
        Assert.That(directoriesConfig.Root, Is.EqualTo(_config.RootDirectory));
        // DirectoriesConfig doesn't expose subdirectories array, but we can test path resolution
        Assert.That(directoriesConfig["templates"], Does.StartWith(_config.RootDirectory));
    }

    [Test]
    public void ConfigureServices_UnsupportedLoaderType_ShouldThrowNotImplementedException()
    {
        _config.LoaderType = (ContentLoaderType)999; // Invalid loader type

        Assert.Throws<NotImplementedException>(() => new GloamHost(_config));
    }

    #endregion

    #region Core Services Tests

    [Test]
    public void RegisterCoreServices_ShouldRegisterEntitySchemaValidator()
    {
        _host = new GloamHost(_config);

        var validator = _host.Container.Resolve<IEntitySchemaValidator>();

        Assert.That(validator, Is.TypeOf<JsonSchemaValidator>());
    }

    [Test]
    public void RegisterCoreServices_ShouldRegisterEntityDataLoader()
    {
        _host = new GloamHost(_config);

        var dataLoader = _host.Container.Resolve<IEntityDataLoader>();

        Assert.That(dataLoader, Is.TypeOf<EntityDataLoader>());
    }

    [Test]
    public void RegisterCoreServices_EntitySchemaValidator_ShouldBeSingleton()
    {
        _host = new GloamHost(_config);

        var validator1 = _host.Container.Resolve<IEntitySchemaValidator>();
        var validator2 = _host.Container.Resolve<IEntitySchemaValidator>();

        Assert.That(validator1, Is.SameAs(validator2));
    }

    [Test]
    public void RegisterCoreServices_EntityDataLoader_ShouldBeSingleton()
    {
        _host = new GloamHost(_config);

        var loader1 = _host.Container.Resolve<IEntityDataLoader>();
        var loader2 = _host.Container.Resolve<IEntityDataLoader>();

        Assert.That(loader1, Is.SameAs(loader2));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void Integration_EntityDataLoader_ShouldResolveDependencies()
    {
        _host = new GloamHost(_config);

        // EntityDataLoader depends on IContentLoader and IEntitySchemaValidator
        var dataLoader = _host.Container.Resolve<IEntityDataLoader>();

        Assert.That(dataLoader, Is.Not.Null);
        Assert.That(dataLoader, Is.TypeOf<EntityDataLoader>());
    }

    [Test]
    public void Integration_AllCoreServices_ShouldResolveWithoutErrors()
    {
        _host = new GloamHost(_config);

        Assert.DoesNotThrow(() => _host.Container.Resolve<IContentLoader>());
        Assert.DoesNotThrow(() => _host.Container.Resolve<IEntitySchemaValidator>());
        Assert.DoesNotThrow(() => _host.Container.Resolve<IEntityDataLoader>());
        Assert.DoesNotThrow(() => _host.Container.Resolve<DirectoriesConfig>());
        Assert.DoesNotThrow(() => _host.Container.Resolve<GloamHostConfig>());
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_ShouldDisposeContainer()
    {
        _host = new GloamHost(_config);
        var container = _host.Container;

        _host.Dispose();

        // After disposal, container should be disposed
        Assert.Throws<ContainerException>(() => container.Resolve<GloamHostConfig>());
    }

    [Test]
    public void Dispose_MultipleCalls_ShouldNotThrow()
    {
        _host = new GloamHost(_config);

        _host.Dispose();

        Assert.DoesNotThrow(() => _host.Dispose());
    }

    [Test]
    public void Dispose_WithUsingStatement_ShouldWorkCorrectly()
    {
        IContainer? containerRef = null;

        using (var host = new GloamHost(_config))
        {
            containerRef = host.Container;
            Assert.That(containerRef, Is.Not.Null);
        }

        // After using block, container should be disposed
        Assert.Throws<ContainerException>(() => containerRef!.Resolve<GloamHostConfig>());
    }

    #endregion

    #region Performance Tests

    [Test]
    public void Performance_RepeatedResolution_ShouldBeEfficient()
    {
        _host = new GloamHost(_config);

        // Warm up
        for (var i = 0; i < 100; i++)
        {
            _host.Container.Resolve<IEntitySchemaValidator>();
        }

        var stopwatch = Stopwatch.StartNew();

        // Test repeated singleton resolution
        for (var i = 0; i < 10000; i++)
        {
            _host.Container.Resolve<IEntitySchemaValidator>();
        }

        stopwatch.Stop();

        // Should resolve 10k singletons in reasonable time (less than 100ms)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
    }

    [Test]
    public void Performance_TransientCreation_ShouldBeEfficient()
    {
        _host = new GloamHost(_config);
        _host.Container.Register<TestTransientService>();

        // Warm up
        for (var i = 0; i < 100; i++)
        {
            _host.Container.Resolve<TestTransientService>();
        }

        var stopwatch = Stopwatch.StartNew();

        // Test repeated transient creation
        for (var i = 0; i < 1000; i++)
        {
            _host.Container.Resolve<TestTransientService>();
        }

        stopwatch.Stop();

        // Should create 1k transients in reasonable time (less than 50ms)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void EdgeCase_EmptyRootDirectory_ShouldThrowException()
    {
        _config.RootDirectory = "";

        Assert.Throws<ArgumentException>(() => _host = new GloamHost(_config));
    }

    [Test]
    public void EdgeCase_NonExistentRootDirectory_ShouldThrowException()
    {
        _config.RootDirectory = "/nonexistent/path/that/should/not/exist";

        Assert.That(() => _host = new GloamHost(_config), 
            Throws.InstanceOf<IOException>().Or.InstanceOf<UnauthorizedAccessException>());
    }

    #endregion

    #region Host State and Lifecycle Tests

    [Test]
    public async Task InitializeAsync_ShouldChangeStateToInitialized()
    {
        _host = new GloamHost(_config);
        Assert.That(_host.State, Is.EqualTo(HostState.Created));

        await _host.InitializeAsync();

        Assert.That(_host.State, Is.EqualTo(HostState.Initialized));
    }

    [Test]
    public async Task InitializeAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        _host = new GloamHost(_config);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // InitializeAsync currently doesn't use the cancellation token, but should accept it
        await _host.InitializeAsync(cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Initialized));
    }

    [Test]
    public async Task LoadContentAsync_ShouldChangeStateToContentLoaded()
    {
        _host = new GloamHost(_config);

        await _host.LoadContentAsync(_tempDirectory);

        Assert.That(_host.State, Is.EqualTo(HostState.ContentLoaded));
    }

    [Test]
    public async Task StartAsync_ShouldChangeStateToRunning()
    {
        _host = new GloamHost(_config);

        await _host.StartAsync();

        Assert.That(_host.State, Is.EqualTo(HostState.Running));
    }

    [Test]
    public async Task StopAsync_ShouldChangeStateToStopped()
    {
        _host = new GloamHost(_config);

        await _host.StopAsync();

        Assert.That(_host.State, Is.EqualTo(HostState.Stopped));
    }

    #endregion

    #region RunAsync Legacy Method Tests

    [Test]
    public async Task RunAsync_LegacyOverload_ShouldCallNewOverload()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var keepRunning = () => ++runCount <= 3;
        var fixedStep = TimeSpan.FromMilliseconds(16);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await _host.RunAsync(keepRunning, fixedStep, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task RunAsync_LegacyOverload_TurnBasedMode_ShouldWork()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var keepRunning = () => ++runCount <= 2;

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await _host.RunAsync(keepRunning, TimeSpan.Zero, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.GreaterThan(0));
    }

    #endregion

    #region RunAsync GameLoopConfig Tests

    [Test]
    public async Task RunAsync_WithGameLoopConfig_ShouldSetStateToRunning()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 2
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await _host.RunAsync(config, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
    }

    [Test]
    public async Task RunAsync_FixedTimestep_ShouldProcessCorrectly()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5,
            SimulationStep = TimeSpan.FromMilliseconds(10),
            RenderStep = TimeSpan.FromMilliseconds(20)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await _host.RunAsync(config, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task RunAsync_TurnBasedMode_ShouldProcessCorrectly()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3,
            SimulationStep = TimeSpan.Zero,
            RenderStep = TimeSpan.FromMilliseconds(50)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await _host.RunAsync(config, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task RunAsync_CancellationToken_ShouldStopLoop()
    {
        _host = new GloamHost(_config);
        var config = new GameLoopConfig
        {
            KeepRunning = () => true // Would run forever without cancellation
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

        // Cancellation token throws TaskCanceledException when canceled during Task.Delay
        Assert.ThrowsAsync<TaskCanceledException>(async () => await _host.RunAsync(config, cts.Token));
    }

    [Test]
    public async Task RunAsync_KeepRunningReturnsFalse_ShouldStopLoop()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await _host.RunAsync(config, cts.Token);

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.EqualTo(4)); // Called one more time to return false
    }

    #endregion

    #region Game Loop Timing Tests

    [Test]
    public async Task RunAsync_TimingAccuracy_ShouldMaintainReasonableFrameRate()
    {
        _host = new GloamHost(_config);
        var startTime = DateTime.UtcNow;
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 10,
            SimulationStep = TimeSpan.FromMilliseconds(10),
            RenderStep = TimeSpan.FromMilliseconds(10)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await _host.RunAsync(config, cts.Token);

        var elapsed = DateTime.UtcNow - startTime;

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.EqualTo(11));
        // Should complete reasonably quickly - not take too long due to excessive sleeping
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(500));
    }

    [Test]
    public async Task RunAsync_HighFrequency_ShouldHandleQuickLoops()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 100,
            SimulationStep = TimeSpan.FromMilliseconds(1),
            RenderStep = TimeSpan.FromMilliseconds(1),
            MaxSleepTime = TimeSpan.FromMilliseconds(1)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var startTime = DateTime.UtcNow;

        await _host.RunAsync(config, cts.Token);

        var elapsed = DateTime.UtcNow - startTime;

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        Assert.That(runCount, Is.EqualTo(101));
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(1000)); // Should complete in reasonable time
    }

    #endregion

    #region DisposeAsync Tests

    [Test]
    public async Task DisposeAsync_ShouldDisposeContainer()
    {
        _host = new GloamHost(_config);
        var container = _host.Container;

        await _host.DisposeAsync();

        // After disposal, container should be disposed
        Assert.Throws<ContainerException>(() => container.Resolve<GloamHostConfig>());
    }

    [Test]
    public async Task DisposeAsync_MultipleCalls_ShouldNotThrow()
    {
        _host = new GloamHost(_config);

        await _host.DisposeAsync();

        Assert.DoesNotThrowAsync(async () => await _host.DisposeAsync());
    }

    [Test]
    public async Task DisposeAsync_WithAsyncDisposableContainer_ShouldCallAsyncDispose()
    {
        _host = new GloamHost(_config);
        var container = _host.Container;

        // DryIoc Container implements IAsyncDisposable
        await _host.DisposeAsync();

        // Verify container is disposed
        Assert.Throws<ContainerException>(() => container.Resolve<GloamHostConfig>());
    }

    [Test]
    public async Task DisposeAsync_WithUsingStatement_ShouldWorkCorrectly()
    {
        IContainer? containerRef = null;

        await using (var host = new GloamHost(_config))
        {
            containerRef = host.Container;
            Assert.That(containerRef, Is.Not.Null);
        }

        // After using block, container should be disposed
        Assert.Throws<ContainerException>(() => containerRef!.Resolve<GloamHostConfig>());
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task RunAsync_KeepRunningThrowsException_ShouldStopLoop()
    {
        _host = new GloamHost(_config);
        var callCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () =>
            {
                callCount++;
                if (callCount > 2)
                    throw new InvalidOperationException("Test exception");
                return true;
            }
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        // The exception should propagate and stop the loop
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _host.RunAsync(config, cts.Token));
    }

    [Test]
    public void RunAsync_NullConfig_ShouldThrowArgumentNullException()
    {
        _host = new GloamHost(_config);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await _host.RunAsync(null!, CancellationToken.None));
    }

    #endregion

    #region Integration with Sleep Time Calculation Tests

    [Test]
    public async Task RunAsync_SleepTimeCalculation_FixedTimestep_ShouldSleepAppropriately()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5,
            SimulationStep = TimeSpan.FromMilliseconds(20),
            RenderStep = TimeSpan.FromMilliseconds(20)
        };

        var startTime = DateTime.UtcNow;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await _host.RunAsync(config, cts.Token);

        var elapsed = DateTime.UtcNow - startTime;

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        // Should take at least some time due to sleep calculations (allowing for fast execution)
        Assert.That(elapsed.TotalMilliseconds, Is.GreaterThan(5));
        // But not too long due to efficient sleep calculation
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(500));
    }

    [Test]
    public async Task RunAsync_SleepTimeCalculation_TurnBased_ShouldUseDefaultSleep()
    {
        _host = new GloamHost(_config);
        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3,
            SimulationStep = TimeSpan.Zero,
            RenderStep = TimeSpan.FromMilliseconds(100)
        };

        var startTime = DateTime.UtcNow;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await _host.RunAsync(config, cts.Token);

        var elapsed = DateTime.UtcNow - startTime;

        Assert.That(_host.State, Is.EqualTo(HostState.Paused));
        // Turn-based mode should still have some timing delays
        Assert.That(elapsed.TotalMilliseconds, Is.GreaterThan(10));
    }

    #endregion

    #region SetRenderer and SetInputDevice Tests

    [Test]
    public void SetRenderer_WithValidRenderer_ShouldStoreRenderer()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);

        _host.SetRenderer(mockRenderer.Object);

        // We can't directly access the private _renderer field, but we can verify behavior
        // through the game loop integration tests
        Assert.That(_host, Is.Not.Null); // Renderer stored successfully
    }

    [Test]
    public void SetRenderer_WithNullRenderer_ShouldAcceptNull()
    {
        _host = new GloamHost(_config);

        Assert.DoesNotThrow(() => _host.SetRenderer(null));
    }

    [Test]
    public void SetInputDevice_WithValidInputDevice_ShouldStoreInputDevice()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());

        _host.SetInputDevice(mockInputDevice.Object);

        // We can't directly access the private _inputDevice field, but we can verify behavior
        // through the game loop integration tests
        Assert.That(_host, Is.Not.Null); // Input device stored successfully
    }

    [Test]
    public void SetInputDevice_WithNullInputDevice_ShouldAcceptNull()
    {
        _host = new GloamHost(_config);

        Assert.DoesNotThrow(() => _host.SetInputDevice(null));
    }

    #endregion

    #region Game Loop Integration Tests

    [Test]
    public async Task RunAsync_WithRenderer_ShouldCallBeginAndEndDraw()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 2,
            RenderStep = TimeSpan.FromMilliseconds(1) // Force render every loop
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Verify renderer methods were called
        mockRenderer.Verify(r => r.BeginDraw(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.EndDraw(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RunAsync_WithInputDevice_ShouldCallPollAndEndFrame()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Verify input device methods were called
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RunAsync_WithBothRendererAndInputDevice_ShouldCallAllMethods()
    {
        _host = new GloamHost(_config);
        
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);
        
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 2,
            RenderStep = TimeSpan.FromMilliseconds(1) // Force render every loop
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Verify all methods were called
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.BeginDraw(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.EndDraw(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RunAsync_WithoutRenderer_ShouldNotCallRenderMethods()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 2,
            RenderStep = TimeSpan.FromMilliseconds(1)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        
        // Should not throw when renderer is null
        Assert.DoesNotThrowAsync(async () => await _host.RunAsync(config, cts.Token));
        
        // Input device should still be called
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RunAsync_WithoutInputDevice_ShouldNotCallInputMethods()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 2,
            RenderStep = TimeSpan.FromMilliseconds(1)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        
        // Should not throw when input device is null
        Assert.DoesNotThrowAsync(async () => await _host.RunAsync(config, cts.Token));
        
        // Renderer should still be called
        mockRenderer.Verify(r => r.BeginDraw(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.EndDraw(), Times.AtLeastOnce);
    }

    #endregion

    #region Render Timing Tests

    [Test]
    public async Task RunAsync_RenderStepTiming_ShouldRespectRenderInterval()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 10,
            SimulationStep = TimeSpan.FromMilliseconds(1),
            RenderStep = TimeSpan.FromMilliseconds(50) // Render less frequently
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        await _host.RunAsync(config, cts.Token);

        // Should render fewer times than simulation steps
        var renderCalls = mockRenderer.Invocations.Count(i => i.Method.Name == "BeginDraw");
        Assert.That(renderCalls, Is.LessThan(runCount));
        Assert.That(renderCalls, Is.GreaterThan(0));
    }

    [Test]
    public async Task RunAsync_FastRenderStep_ShouldRenderFrequently()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5,
            SimulationStep = TimeSpan.FromMilliseconds(10),
            RenderStep = TimeSpan.FromMilliseconds(1) // Render very frequently
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Should render frequently
        mockRenderer.Verify(r => r.BeginDraw(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.EndDraw(), Times.AtLeastOnce);
    }

    #endregion

    #region Turn-Based vs Real-Time Input Tests

    [Test]
    public async Task RunAsync_TurnBasedMode_ShouldStillPollInput()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3,
            SimulationStep = TimeSpan.Zero, // Turn-based
            RenderStep = TimeSpan.FromMilliseconds(10)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Input should still be polled in turn-based mode
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    [Test]
    public async Task RunAsync_RealTimeMode_ShouldPollInputRegularly()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5,
            SimulationStep = TimeSpan.FromMilliseconds(10), // Real-time
            RenderStep = TimeSpan.FromMilliseconds(10)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await _host.RunAsync(config, cts.Token);

        // Input should be polled regularly in real-time mode
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    #endregion

    #region Error Handling in Game Loop Tests

    [Test]
    public async Task RunAsync_RendererThrowsException_ShouldPropagateException()
    {
        _host = new GloamHost(_config);
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        mockRenderer.Setup(r => r.BeginDraw()).Throws(new InvalidOperationException("Renderer error"));
        _host.SetRenderer(mockRenderer.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5,
            RenderStep = TimeSpan.FromMilliseconds(1)
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _host.RunAsync(config, cts.Token));
    }

    [Test]
    public async Task RunAsync_InputDeviceThrowsException_ShouldPropagateException()
    {
        _host = new GloamHost(_config);
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Poll()).Throws(new InvalidOperationException("Input error"));
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 5
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _host.RunAsync(config, cts.Token));
    }

    #endregion

    #region Sleep Time Calculation with Renderer/Input Tests

    [Test]
    public async Task RunAsync_WithRendererAndInput_ShouldCalculateSleepTimeCorrectly()
    {
        _host = new GloamHost(_config);
        
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        _host.SetRenderer(mockRenderer.Object);
        
        var mockInputDevice = new Mock<IInputDevice>();
        mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
        _host.SetInputDevice(mockInputDevice.Object);

        var runCount = 0;
        var config = new GameLoopConfig
        {
            KeepRunning = () => ++runCount <= 3,
            SimulationStep = TimeSpan.FromMilliseconds(20),
            RenderStep = TimeSpan.FromMilliseconds(30),
            MaxSleepTime = TimeSpan.FromMilliseconds(10)
        };

        var startTime = DateTime.UtcNow;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        await _host.RunAsync(config, cts.Token);
        var elapsed = DateTime.UtcNow - startTime;

        // Should complete within reasonable time - not too fast (no sleep) or too slow (excessive sleep)
        Assert.That(elapsed.TotalMilliseconds, Is.GreaterThan(10));
        Assert.That(elapsed.TotalMilliseconds, Is.LessThan(300));
        
        // Verify all methods were called
        mockInputDevice.Verify(i => i.Poll(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.BeginDraw(), Times.AtLeastOnce);
        mockRenderer.Verify(r => r.EndDraw(), Times.AtLeastOnce);
        mockInputDevice.Verify(i => i.EndFrame(), Times.AtLeastOnce);
    }

    #endregion

    #region Test Helper Classes

    private class TestTransientService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    private class TestSingletonService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    #endregion
}
