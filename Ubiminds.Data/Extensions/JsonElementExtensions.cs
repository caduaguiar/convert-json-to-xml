using System.Text.Json;

namespace Ubiminds.Data.Extensions;

public static class JsonElementExtensions
{
    public static JsonElement GetPropertyOrDefault(this JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Name == propertyName)
            {
                return property.Value;
            }
        }

        return default;
    }
}
