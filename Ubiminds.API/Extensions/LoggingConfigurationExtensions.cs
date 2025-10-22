namespace Ubiminds.API.Extensions;

public static class LoggingConfigurationExtensions
{
    public static ILoggingBuilder AddLoggingConfiguration(this ILoggingBuilder logging, IConfiguration configuration)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        var loggingSection = configuration.GetSection("Logging");
        if (loggingSection.Exists())
        {
            logging.AddConfiguration(loggingSection);
        }

        return logging;
    }
}
