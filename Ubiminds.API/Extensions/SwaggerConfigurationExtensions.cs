using Microsoft.OpenApi.Models;

namespace Ubiminds.API.Extensions;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ubiminds JSON to XML Converter API",
                Version = "v1",
                Description = "An ASP.NET Core Web API for converting JSON documents to XML format with validation rules",
                Contact = new OpenApiContact
                {
                    Name = "Carlos Aguiar",
                    Email = "carlos.aguiar@ubiminds.com"
                }
            });

            options.EnableAnnotations();

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
