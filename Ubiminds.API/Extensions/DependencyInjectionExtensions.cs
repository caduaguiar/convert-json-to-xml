using Ubiminds.Data.Configuration;
using Ubiminds.Data.Services;

namespace Ubiminds.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ValidationOptions>(
            configuration.GetSection(ValidationOptions.SectionName));

        services.Configure<XmlConversionOptions>(
            configuration.GetSection(XmlConversionOptions.SectionName));

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPublishedItemXmlBuilder, PublishedItemXmlBuilder>();
        services.AddScoped<IJsonToXmlConverter, JsonToXmlConverter>();

        return services;
    }
}
