using Gloam.Runtime.Types;
using Serilog.Events;

namespace Gloam.Runtime.Extensions;

/// <summary>
/// Extension methods for converting between Gloam log levels and Serilog log levels
/// </summary>
public static class LogLevelExtensions
{
    /// <summary>
    /// Converts a LogLevelType to the corresponding Serilog LogEventLevel
    /// </summary>
    /// <param name="logLevel">The Gloam log level to convert</param>
    /// <returns>The corresponding Serilog LogEventLevel</returns>
    public static LogEventLevel ToSerilogLogLevel(this LogLevelType logLevel)
    {
        return logLevel switch
        {
            LogLevelType.Trace       => LogEventLevel.Verbose,
            LogLevelType.Debug       => LogEventLevel.Debug,
            LogLevelType.Information => LogEventLevel.Information,
            LogLevelType.Warning     => LogEventLevel.Warning,
            LogLevelType.Error       => LogEventLevel.Error,
            _                        => LogEventLevel.Information
        };
    }
}
