using DryIoc;
using Gloam.Data.Content;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Gloam.Data.Loaders;
using Gloam.Data.Validators;
using Gloam.Runtime.Config;
using Gloam.Runtime.Extensions;
using Gloam.Runtime.Interfaces;
using Gloam.Runtime.Types;
using Mosaic.Engine.Directories;
using Serilog;
using Serilog.Formatting.Compact;

namespace Gloam.Runtime;

/// <summary>
/// Optimized DryIoc container host for roguelike engines.
/// Features performance optimizations for high-frequency object creation,
/// memory management, and game loop scoping.
/// </summary>
public class GloamHost : IGloamHost
{
    /// <summary>
    /// Gets the current state of the host
    /// </summary>
    public HostState State { get; private set; }

    private readonly GloamHostConfig _config;

    /// <summary>
    /// Initializes a new instance of GloamHost with the specified configuration
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
    /// Gets the dependency injection container
    /// </summary>
    public IContainer Container { get; }

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
            throw new NotImplementedException("Only FileSystem content loader is implemented.");
        }


        RegisterCoreServices();
    }

    private void RegisterCoreServices()
    {
        Container.Register<IEntitySchemaValidator, JsonSchemaValidator>(Reuse.Singleton);
        Container.Register<IEntityDataLoader, EntityDataLoader>(Reuse.Singleton);
    }


    /// <summary>
    /// Disposes the host and releases all resources
    /// </summary>
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the host and releases all resources
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
    /// Initializes the host asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the initialization operation</returns>
    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {
        State = HostState.Initialized;
    }

    /// <summary>
    /// Loads content from the specified root directory
    /// </summary>
    /// <param name="contentRoot">The root directory for content loading</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the content loading operation</returns>
    public async ValueTask LoadContentAsync(string contentRoot, CancellationToken ct = default)
    {
        State = HostState.ContentLoaded;
    }

    /// <summary>
    /// Starts the host and begins the game loop
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the start operation</returns>
    public async Task StartAsync(CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        State = HostState.Running;
    }

    /// <summary>
    /// Runs the game loop with the specified timing and condition
    /// </summary>
    /// <param name="keepRunning">Function that returns true while the game should continue running</param>
    /// <param name="fixedStep">Fixed time step for the game loop</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the game loop execution</returns>
    public Task RunAsync(Func<bool> keepRunning, TimeSpan fixedStep, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the host and game loop gracefully
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the stop operation</returns>
    public async Task StopAsync(CancellationToken ct = default)
    {
        State = HostState.Stopped;
    }
}
