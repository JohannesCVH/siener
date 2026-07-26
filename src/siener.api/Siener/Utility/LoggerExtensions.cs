using System.Text;
using Serilog.Context;

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
        
        using (LogContext.PushProperty("MethodName", methodName))
        {
            StringBuilder sb = new StringBuilder();

            int count = 0;
            object[] objArgs = new object[args.Count];
            foreach (var (key, value) in args)
            {
                sb.Append(key + ": {" + key + "}");
                objArgs[count] = value;
                count++;
                if (count != args.Count)
                    sb.Append(", ");
            }

            if (logType == LogType.Information)
                logger.LogInformation(message + (args.Count > 0 ? " | " + sb.ToString() : string.Empty), objArgs);
            if (logType == LogType.Error)
                logger.LogError(message + (args.Count > 0 ? " | " + sb.ToString() : string.Empty), objArgs);
        }
    }
}