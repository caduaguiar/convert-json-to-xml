using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ubiminds.Data.Extensions;

namespace Ubiminds.Data.Services;

public class JsonToXmlConverter(
    ILogger<JsonToXmlConverter> logger,
    IPublishedItemXmlBuilder xmlBuilder) : IJsonToXmlConverter
{
    public string Convert(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);

        logger.LogInformationStructured(nameof(JsonToXmlConverter), nameof(Convert), "Starting JSON to XML conversion");

        try
        {
            var result = xmlBuilder.BuildXml(jsonDocument);
            logger.LogInformationStructured(nameof(JsonToXmlConverter), nameof(Convert), $"Conversion completed successfully with {result.Length} characters");
            return result;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogErrorStructured(nameof(JsonToXmlConverter), nameof(Convert), "Unexpected error during conversion", ex);
            throw new InvalidOperationException($"Failed to convert JSON to XML: {ex.Message}", ex);
        }
    }
}
