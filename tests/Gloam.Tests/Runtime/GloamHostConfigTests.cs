using Gloam.Runtime.Config;
using Gloam.Runtime.Types;

namespace Gloam.Tests.Runtime;

public class GloamHostConfigTests
{
    [Test]
    public void Constructor_WithDefaults_ShouldHaveCorrectValues()
    {
        var config = new GloamHostConfig();

        Assert.That(config.AppName, Is.EqualTo("GloamApp"));
        Assert.That(config.AppVersion, Is.EqualTo("1.0.0"));
        Assert.That(config.LoaderType, Is.EqualTo(ContentLoaderType.FileSystem));
        Assert.That(config.LogLevel, Is.EqualTo(LogLevelType.Information));
        Assert.That(config.EnableConsoleLogging, Is.True);
        Assert.That(config.EnableFileLogging, Is.False);
    }

    [Test]
    public void AppName_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.AppName = "MyRoguelike";

        Assert.That(config.AppName, Is.EqualTo("MyRoguelike"));
    }

    [Test]
    public void AppVersion_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.AppVersion = "2.1.0";

        Assert.That(config.AppVersion, Is.EqualTo("2.1.0"));
    }

    [Test]
    public void RootDirectory_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.RootDirectory = "/path/to/game";

        Assert.That(config.RootDirectory, Is.EqualTo("/path/to/game"));
    }

    [Test]
    public void LoaderType_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.LoaderType = ContentLoaderType.FileSystem;

        Assert.That(config.LoaderType, Is.EqualTo(ContentLoaderType.FileSystem));
    }

    [Test]
    public void LogLevel_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.LogLevel = LogLevelType.Debug;

        Assert.That(config.LogLevel, Is.EqualTo(LogLevelType.Debug));
    }

    [Test]
    public void EnableConsoleLogging_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.EnableConsoleLogging = false;

        Assert.That(config.EnableConsoleLogging, Is.False);
    }

    [Test]
    public void EnableFileLogging_ShouldBeSettable()
    {
        var config = new GloamHostConfig();
        config.EnableFileLogging = true;

        Assert.That(config.EnableFileLogging, Is.True);
    }

    [Test]
    public void InitializerSyntax_ShouldWork()
    {
        var config = new GloamHostConfig
        {
            AppName = "TestGame",
            AppVersion = "0.1.0",
            LoaderType = ContentLoaderType.FileSystem,
            LogLevel = LogLevelType.Debug,
            EnableConsoleLogging = false,
            EnableFileLogging = true,
            RootDirectory = "/test/path"
        };

        Assert.That(config.AppName, Is.EqualTo("TestGame"));
        Assert.That(config.AppVersion, Is.EqualTo("0.1.0"));
        Assert.That(config.LoaderType, Is.EqualTo(ContentLoaderType.FileSystem));
        Assert.That(config.LogLevel, Is.EqualTo(LogLevelType.Debug));
        Assert.That(config.EnableConsoleLogging, Is.False);
        Assert.That(config.EnableFileLogging, Is.True);
        Assert.That(config.RootDirectory, Is.EqualTo("/test/path"));
    }

    [Test]
    public void RootDirectory_ShouldDefaultToNull()
    {
        var config = new GloamHostConfig();

        Assert.That(config.RootDirectory, Is.Null);
    }
}