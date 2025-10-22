using System.Text.Json;

namespace Ubiminds.Data.Services;

public interface IPublishedItemXmlBuilder
{
    string BuildXml(JsonDocument jsonDocument);
}
