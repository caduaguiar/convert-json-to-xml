using Microsoft.AspNetCore.Mvc;
using Ubiminds.API.Contracts;

namespace Ubiminds.API.Extensions;

public static class ApiConfigurationExtensions
{
    public static IMvcBuilder AddApiConfiguration(this IServiceCollection services)
    {
        return services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse;
            });
    }

    private static IActionResult CreateInvalidModelStateResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
            .ToList();

        var traceId = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(new ErrorResponse
        {
            Error = "Validation failed",
            Details = errors,
            TraceId = traceId
        });
    }
}
