using DryIoc;
using Gloam.Data.Content;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Gloam.Data.Loaders;
using Gloam.Data.Validators;
using Gloam.Runtime.Config;
using Gloam.Runtime.Types;
using Mosaic.Engine.Directories;

namespace Gloam.Runtime;

/// <summary>
/// Optimized DryIoc container host for roguelike engines.
/// Features performance optimizations for high-frequency object creation,
/// memory management, and game loop scoping.
/// </summary>
public class GloamHost : IDisposable
{
    private readonly GloamHostConfig _config;


    public GloamHost(GloamHostConfig config)
    {
        _config = config;
        Container = CreateContainer();
        ConfigureServices();
    }

    public IContainer Container { get; }

    private static IContainer CreateContainer()
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

    private void ConfigureServices()
    {
        // Core engine services as singletons for performance
        Container.RegisterInstance(_config);

        if (_config.LoaderType == ContentLoaderType.FileSystem)
        {
            var directoryConfig = new DirectoriesConfig(_config.RootDirectory, ["templates"]);
            Container.Register<IContentLoader, FileSystemContentLoader>();
            Container.RegisterInstance(directoryConfig);
        }
        else
        {
            throw new NotImplementedException("Only FileSystem content loader is implemented.");
        }


        // Note: _directoriesConfig is currently always null in constructor
        // _container.RegisterInstance(_directoriesConfig);

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
}
