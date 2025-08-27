using System.Diagnostics;
using DryIoc;
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
        _config.RootDirectory = "/path/that/does/not/exist";

        Assert.Throws<IOException>(() => _host = new GloamHost(_config));
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
