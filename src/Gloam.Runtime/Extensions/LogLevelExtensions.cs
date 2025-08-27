using Gloam.Runtime.Types;
using Serilog.Events;

namespace Gloam.Runtime.Extensions;

public static class LogLevelExtensions
{
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
