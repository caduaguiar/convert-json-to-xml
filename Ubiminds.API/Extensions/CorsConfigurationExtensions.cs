using Microsoft.Extensions.DependencyInjection;

namespace Ubiminds.API.Extensions;

public static class CorsConfigurationExtensions
{
    private const string PolicyName = "AllowReactApp";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors");
        var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
        var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ?? ["GET", "POST"];
        var allowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>() ?? ["Content-Type"];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders)
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}
