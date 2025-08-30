using Gloam.Runtime.Types;

namespace Gloam.Runtime.Config;

/// <summary>
///     Configuration class for the Gloam runtime host, containing application settings and runtime options
/// </summary>
public class GloamHostConfig
{
    /// <summary>
    ///     Gets or sets the application name
    /// </summary>
    public string AppName { get; set; } = "GloamApp";

    /// <summary>
    ///     Gets or sets the application version
    /// </summary>
    public string AppVersion { get; set; } = "1.0.0";


    /// <summary>
    ///     Gets or sets the root directory for content loading
    /// </summary>
    public string RootDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the type of content loader to use
    /// </summary>
    public ContentLoaderType LoaderType { get; set; } = ContentLoaderType.FileSystem;

    /// <summary>
    ///     Gets or sets the minimum log level for filtering messages
    /// </summary>
    public LogLevelType LogLevel { get; set; } = LogLevelType.Information;

    /// <summary>
    ///     Gets or sets whether to enable console logging output
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to enable file-based logging output
    /// </summary>
    public bool EnableFileLogging { get; set; }
}
