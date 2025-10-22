using System.Text.Json;

namespace Ubiminds.Data.Services;

public interface IJsonToXmlConverter
{
    string Convert(JsonDocument jsonDocument);
}
