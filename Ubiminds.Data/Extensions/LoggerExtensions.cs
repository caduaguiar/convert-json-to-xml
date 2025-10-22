using Microsoft.Extensions.Logging;

namespace Ubiminds.Data.Extensions;

public static class LoggerExtensions
{
    public static void LogStructured(
        this ILogger logger,
        LogLevel logLevel,
        string className,
        string methodName,
        string message,
        Exception? exception = null)
    {
        var formattedMessage = "[{ClassName}] - [{MethodName}] - {Message}";

        if (exception != null)
        {
            logger.Log(logLevel, exception, formattedMessage, className, methodName, message);
        }
        else
        {
            logger.Log(logLevel, formattedMessage, className, methodName, message);
        }
    }

    public static void LogInformationStructured(this ILogger logger, string className, string methodName, string message) =>
        logger.LogStructured(LogLevel.Information, className, methodName, message);

    public static void LogWarningStructured(this ILogger logger, string className, string methodName, string message) =>
        logger.LogStructured(LogLevel.Warning, className, methodName, message);

    public static void LogDebugStructured(this ILogger logger, string className, string methodName, string message) =>
        logger.LogStructured(LogLevel.Debug, className, methodName, message);

    public static void LogErrorStructured(this ILogger logger, string className, string methodName, string message, Exception exception) =>
        logger.LogStructured(LogLevel.Error, className, methodName, message, exception);
}
