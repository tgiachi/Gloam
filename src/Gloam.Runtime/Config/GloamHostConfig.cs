using Gloam.Runtime.Types;

namespace Gloam.Runtime.Config;

/// <summary>
/// Configuration class for the Gloam runtime host, containing application settings and runtime options
/// </summary>
public class GloamHostConfig
{
    public string AppName { get; set; } = "GloamApp";

    public string AppVersion { get; set; } = "1.0.0";

    public RuntimeConfig RuntimeConfig { get; set; } = new();

    public string RootDirectory { get; set; }

    public ContentLoaderType LoaderType { get; set; } = ContentLoaderType.FileSystem;

    public LogLevelType LogLevel { get; set; } = LogLevelType.Information;

    public bool EnableConsoleLogging { get; set; } = true;

    public bool EnableFileLogging { get; set; }
}
