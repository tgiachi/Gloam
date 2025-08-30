using System.Diagnostics;
using DryIoc;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Data.Content;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Gloam.Data.Loaders;
using Gloam.Data.Validators;
using Gloam.Runtime.Config;
using Gloam.Runtime.Extensions;
using Gloam.Runtime.Interfaces;
using Gloam.Runtime.Services;
using Gloam.Runtime.Types;
using Gloam.Core.Directories;
using Serilog;
using Serilog.Formatting.Compact;

namespace Gloam.Runtime;

/// <summary>
///     Optimized DryIoc container host for roguelike engines.
///     Features performance optimizations for high-frequency object creation,
///     memory management, and game loop scoping.
/// </summary>
public class GloamHost : IGloamHost
{
    private readonly GloamHostConfig _config;
    private IInputDevice? _inputDevice;
    private IRenderer? _renderer;

    // Loop state for external mode
    private long _startTimestamp;
    private long _lastRenderTimestamp;
    private bool _isFirstFrame = true;
    private ILayerRenderingManager? _layerRenderingManager;
    private ISceneManager? _sceneManager;


    /// <summary>
    ///     Initializes a new instance of GloamHost with the specified configuration
    /// </summary>
    /// <param name="config">The host configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    public GloamHost(GloamHostConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        Container = CreateContainer();
        ConfigureServices();
        ConfigureLogging();
        State = HostState.Created;
    }

    /// <summary>
    ///     Gets the dependency injection container
    /// </summary>
    public IContainer Container { get; }

    /// <summary>
    ///     Gets the current state of the host
    /// </summary>
    public HostState State { get; private set; }

    public ILayerRenderingManager LayerRenderingManager => Container.Resolve<ILayerRenderingManager>();

    public ISceneManager SceneManager => Container.Resolve<ISceneManager>();


    /// <summary>
    ///     Disposes the host and releases all resources
    /// </summary>
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Asynchronously disposes the host and releases all resources
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (Container is IAsyncDisposable containerAsyncDisposable)
        {
            await containerAsyncDisposable.DisposeAsync();
        }
        else
        {
            Container.Dispose();
        }

        GC.SuppressFinalize(this);
    }


    /// <summary>
    ///     Initializes the host asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the initialization operation</returns>
    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {
        State = HostState.Initialized;
    }

    /// <summary>
    ///     Loads content from the specified root directory
    /// </summary>
    /// <param name="contentRoot">The root directory for content loading</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the content loading operation</returns>
    public async ValueTask LoadContentAsync(string contentRoot, CancellationToken ct = default)
    {
        State = HostState.ContentLoaded;
    }

    /// <summary>
    ///     Starts the host and begins the game loop
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        State = HostState.Running;
    }

    /// <summary>
    ///     Legacy overload for backward compatibility. Runs the roguelike game loop
    /// </summary>
    /// <param name="keepRunning">Function that returns true while the game should continue running</param>
    /// <param name="fixedStep">Ignored in roguelike mode (kept for compatibility)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the game loop execution</returns>
    public async Task RunAsync(Func<bool> keepRunning, TimeSpan fixedStep, CancellationToken ct)
    {
        var config = new GameLoopConfig
        {
            KeepRunning = keepRunning
        };

        await RunAsync(config, ct);
    }


    /// <summary>
    ///     Stops the host and game loop gracefully
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken ct = default)
    {
        State = HostState.Stopped;
    }

    public void SetRenderer(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public void SetInputDevice(IInputDevice inputDevice)
    {
        _inputDevice = inputDevice;
    }

    private static Container CreateContainer()
    {
        var rules = Rules.Default
            // Performance optimizations for roguelike engines
            .WithoutThrowOnRegisteringDisposableTransient()
            .WithoutEagerCachingSingletonForFasterAccess()
            .WithTrackingDisposableTransients()
            .WithUseInterpretation() // Faster than compilation for frequent short-lived objects
            .WithoutImplicitCheckForReuseMatchingScope()

            // Memory optimizations
            .WithoutThrowIfDependencyHasShorterReuseLifespan()

            // Auto concrete type resolution for entity/component creation
            .WithAutoConcreteTypeResolution()

            // Custom scope for game loop lifecycle management
            .WithDefaultReuse(Reuse.Transient);

        return new Container(rules);
    }

    private void ConfigureLogging()
    {
        var loggingConfiguration = new LoggerConfiguration().MinimumLevel.Is(_config.LogLevel.ToSerilogLogLevel());

        if (_config.EnableConsoleLogging)
        {
            loggingConfiguration.WriteTo.Console();
        }

        if (_config.EnableFileLogging)
        {
            loggingConfiguration.WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(Directory.GetCurrentDirectory(), "logs", "gloam_runtime_.log"),
                rollingInterval: RollingInterval.Day
            );
        }

        Log.Logger = loggingConfiguration.CreateLogger();
    }

    private void ConfigureServices()
    {
        // Core engine services as singletons for performance
        Container.RegisterInstance(_config);

        if (_config.LoaderType == ContentLoaderType.FileSystem)
        {
            var directoryConfig = new DirectoriesConfig(_config.RootDirectory, ["templates", "scripts"]);
            Container.RegisterInstance(directoryConfig);

            // Register FileSystemContentLoader with basePath from config
            var contentLoader = new FileSystemContentLoader(_config.RootDirectory);
            Container.RegisterInstance<IContentLoader>(contentLoader);
        }
        else
        {
            throw new NotSupportedException(
                $"Content loader type '{_config.LoaderType}' is not supported. " +
                $"Currently supported loaders: {ContentLoaderType.FileSystem}. " +
                $"Please use FileSystem loader or implement a custom loader.");
        }


        RegisterCoreServices();
    }

    private void RegisterCoreServices()
    {
        Container.Register<IEntitySchemaValidator, JsonSchemaValidator>(Reuse.Singleton);
        Container.Register<IEntityDataLoader, EntityDataLoader>(Reuse.Singleton);


        // RENDERING STUFF
        // Note: RegisterMany will be used when concrete implementations are available
        // For now, just register the manager since there are no concrete ILayerRenderer implementations yet

        // Register the layer rendering manager
        Container.Register<ILayerRenderingManager, LayerRenderingManager>(Reuse.Singleton);

        // Register the scene manager
        Container.Register<ISceneManager, SceneManager>(Reuse.Singleton);
    }

    /// <summary>
    ///     Runs the game loop with the specified configuration
    /// </summary>
    /// <param name="config">Game loop configuration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the game loop execution</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    public async Task RunAsync(GameLoopConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        State = HostState.Running;

        // Initialize managers for both modes
        _layerRenderingManager = Container.Resolve<ILayerRenderingManager>();
        _sceneManager = Container.Resolve<ISceneManager>();

        if (config.LoopMode == LoopMode.Internal)
        {
            // Initialize timestamps for internal loop
            _startTimestamp = Stopwatch.GetTimestamp();
            _lastRenderTimestamp = _startTimestamp;
            _isFirstFrame = true;

            // Run internal loop
            while (config.KeepRunning() && !ct.IsCancellationRequested)
            {
                var now = Stopwatch.GetTimestamp();

                // Poll input device (optimized: only poll if device exists)
                if (_inputDevice != null)
                {
                    _inputDevice.Poll();
                }

                // Update current scene
                await _sceneManager.UpdateCurrentSceneAsync(ct);

                // Rendering (if it's time to render) - optimized timestamp calculation
                // Performance optimization: avoid redundant null checks and timestamp calculations
                var shouldRender = _isFirstFrame || _renderer == null;
                TimeSpan timeSinceLastRender = TimeSpan.Zero;

                if (!shouldRender && _renderer != null)
                {
                    timeSinceLastRender = Stopwatch.GetElapsedTime(_lastRenderTimestamp, now);
                    shouldRender = timeSinceLastRender >= config.RenderStep;
                }

                if (shouldRender && _renderer != null)
                {
                    _renderer.BeginDraw();

                    // Render all layers using the layer rendering manager
                    if (_layerRenderingManager != null && _inputDevice != null)
                    {
                        var renderContext = RenderLayerContext.Create(
                            _renderer,
                            _inputDevice,
                            _startTimestamp,
                            now,
                            timeSinceLastRender,
                            _isFirstFrame,
                            config.RenderStep
                        );

                        await _layerRenderingManager.RenderAllLayersAsync(renderContext, ct);
                    }

                    _renderer.EndDraw();
                    _lastRenderTimestamp = now;
                }

                _isFirstFrame = false;

                // End frame cleanup for input
                _inputDevice?.EndFrame();

                // Sleep to avoid consuming too much CPU - but only if we didn't render
                // This keeps transitions smooth while still saving CPU when idle
                if (!shouldRender)
                {
                    var sleepMs = (int)config.SleepTime.TotalMilliseconds;
                    if (sleepMs > 0)
                    {
                        await Task.Delay(sleepMs, ct);
                    }
                }
            }
        }
        else
        {
            // External mode: Keep running state for external loop control
            // Don't change state here as external code manages the loop
        }

        // Only set to Paused for internal mode when loop ends
        if (config.LoopMode == Types.LoopMode.Internal)
        {
            State = HostState.Paused;
        }
    }

    /// <summary>
    ///     Executes a single game loop iteration. Can be called externally when LoopMode is External.
    /// </summary>
    /// <param name="config">Game loop configuration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the loop iteration</returns>
    /// <exception cref="InvalidOperationException">Thrown when called in Internal mode or when host is not running</exception>
    public async Task LoopAsync(GameLoopConfig config, CancellationToken ct = default)
    {
        if (config.LoopMode == Types.LoopMode.Internal)
        {
            throw new InvalidOperationException("LoopAsync cannot be called when LoopMode is Internal. Use RunAsync instead.");
        }

        if (State != HostState.Running)
        {
            throw new InvalidOperationException("Host must be in Running state to execute loop iterations.");
        }

        // Initialize managers on first call
        _layerRenderingManager ??= Container.Resolve<ILayerRenderingManager>();
        _sceneManager ??= Container.Resolve<ISceneManager>();

        // Initialize timestamps on first call
        if (_isFirstFrame)
        {
            _startTimestamp = Stopwatch.GetTimestamp();
            _lastRenderTimestamp = _startTimestamp;
        }

        var now = Stopwatch.GetTimestamp();

        // Poll input device (optimized: only poll if device exists)
        if (_inputDevice != null)
        {
            _inputDevice.Poll();
        }

        // Update current scene
        await _sceneManager.UpdateCurrentSceneAsync(ct);

        // Rendering (if it's time to render)
        var timeSinceLastRender = Stopwatch.GetElapsedTime(_lastRenderTimestamp, now);
        var shouldRender = (_isFirstFrame || timeSinceLastRender >= config.RenderStep) && _renderer != null;
        if (shouldRender)
        {
            _renderer.BeginDraw();

            // Render all layers using the layer rendering manager
            if (_layerRenderingManager != null && _inputDevice != null)
            {
                var renderContext = RenderLayerContext.Create(
                    _renderer,
                    _inputDevice,
                    _startTimestamp,
                    now,
                    timeSinceLastRender,
                    _isFirstFrame,
                    config.RenderStep
                );

                await _layerRenderingManager.RenderAllLayersAsync(renderContext, ct);
            }

            _renderer.EndDraw();
            _lastRenderTimestamp = now;
        }

        _isFirstFrame = false;

        // End frame cleanup for input
        _inputDevice?.EndFrame();

        // Handle timing in external mode if requested - but only if we didn't render
        if (config.HandleTimingInExternalMode && !shouldRender && config.SleepTime.TotalMilliseconds > 0)
        {
            await Task.Delay((int)config.SleepTime.TotalMilliseconds, ct);
        }
    }

    /// <summary>
    ///     Resets the internal loop state. Useful when switching between loop modes or restarting.
    /// </summary>
    public void ResetLoopState()
    {
        _startTimestamp = 0;
        _lastRenderTimestamp = 0;
        _isFirstFrame = true;
        _layerRenderingManager = null;
        _sceneManager = null;
    }

    /// <summary>
    ///     Gets the current loop state information for debugging purposes
    /// </summary>
    /// <returns>A tuple containing loop state information</returns>
    public (bool IsFirstFrame, TimeSpan TimeSinceStart, TimeSpan TimeSinceLastRender) GetLoopState()
    {
        if (_startTimestamp == 0)
        {
            return (true, TimeSpan.Zero, TimeSpan.Zero);
        }

        var now = Stopwatch.GetTimestamp();
        var timeSinceStart = Stopwatch.GetElapsedTime(_startTimestamp, now);
        var timeSinceLastRender = Stopwatch.GetElapsedTime(_lastRenderTimestamp, now);

        return (_isFirstFrame, timeSinceStart, timeSinceLastRender);
    }
}
