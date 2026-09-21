using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace Siener.Utility;

public enum LogType
{
    Information = 0,
    Error = 1
}

public static class LoggerExtensions
{       
    public static void LogMessage(this ILogger logger, LogType logType, string methodName, string? message, Dictionary<string, string>? args = default)
    {
        if (args is null) args = new();
        
        var enrichers = args.Select(kvp => new PropertyEnricher(kvp.Key, kvp.Value)).Cast<ILogEventEnricher>().ToArray();
        
        using (LogContext.PushProperty("MethodName", methodName))
        using (LogContext.Push(enrichers))
        {
            if (logType == LogType.Information)
                logger.LogInformation(message);
            if (logType == LogType.Error)
                logger.LogError(message);
        }
    }
}