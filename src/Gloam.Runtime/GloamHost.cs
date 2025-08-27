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
    public HostState State { get; private set; }

    private readonly GloamHostConfig _config;


    public GloamHost(GloamHostConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        Container = CreateContainer();
        ConfigureServices();
        ConfigureLogging();
        State = HostState.Created;
    }

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


    // Cleanup method
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }

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


    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {
        State = HostState.Initialized;
    }

    public async ValueTask LoadContentAsync(string contentRoot, CancellationToken ct = default)
    {
        State = HostState.ContentLoaded;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        State = HostState.Running;
    }

    public Task RunAsync(Func<bool> keepRunning, TimeSpan fixedStep, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        State = HostState.Stopped;
    }
}
