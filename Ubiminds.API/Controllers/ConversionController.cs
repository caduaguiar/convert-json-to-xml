using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ubiminds.API.Contracts;
using Ubiminds.Data.Services;

namespace Ubiminds.API.Controllers;

/// <summary>
/// Controller responsible for JSON to XML conversion operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Operations for converting JSON to XML")]
[Produces("application/json")]
public class ConversionController(
    IValidationService validationService,
    IJsonToXmlConverter jsonToXmlConverter,
    ILogger<ConversionController> logger)
    : ControllerBase
{
    private const string LogPrefix = "[ConversionController] - [ConvertJsonToXml]";

    [HttpPost("json-to-xml")]
    [Consumes("application/json")]
    [Produces("application/xml", "application/json")]
    [SwaggerOperation(
        Summary = "Converts a JSON document to XML format with validation",
        Description = "Validates the input JSON against specific business rules and converts it to well-formatted XML",
        OperationId = "ConvertJsonToXml"
    )]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult ConvertJsonToXml([FromBody] JsonElement jsonInput)
    {
        var traceId = HttpContext.TraceIdentifier;
        logger.LogInformation($"{LogPrefix} - Processing conversion request with TraceId: {traceId}");

        if (jsonInput.ValueKind == JsonValueKind.Undefined || jsonInput.ValueKind == JsonValueKind.Null)
        {
            logger.LogWarning($"{LogPrefix} - Received null or undefined JSON, TraceId: {traceId}");
            return BadRequest(new ErrorResponse
            {
                Error = "JSON document cannot be null",
                TraceId = traceId
            });
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonInput.GetRawText());

            logger.LogDebug($"{LogPrefix} - Starting validation, TraceId: {traceId}");

            var validationError = ValidateDocument(jsonDocument, traceId);
            if (validationError != null)
                return validationError;

            var xmlResult = jsonToXmlConverter.Convert(jsonDocument);

            logger.LogInformation($"{LogPrefix} - Conversion completed successfully, TraceId: {traceId}");

            return Content(xmlResult, "application/xml");
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                $"{LogPrefix} - JSON parsing error occurred, TraceId: {traceId}");

            return BadRequest(new ErrorResponse
            {
                Error = $"Invalid JSON format: {ex.Message}",
                TraceId = traceId
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(
                ex,
                $"{LogPrefix} - Invalid operation during conversion, TraceId: {traceId}");

            return BadRequest(new ErrorResponse
            {
                Error = $"Conversion error: {ex.Message}",
                TraceId = traceId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                $"{LogPrefix} - Unexpected error during conversion, TraceId: {traceId}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Error = "An unexpected error occurred during conversion. Please contact support if the issue persists.",
                    TraceId = traceId
                });
        }
    }

    private IActionResult? ValidateDocument(JsonDocument jsonDocument, string traceId)
    {
        var validationResult = validationService.Validate(jsonDocument);

        if (!validationResult.IsValid)
        {
            logger.LogWarning(
                $"{LogPrefix} - Validation failed with error: {validationResult.ErrorMessage}, TraceId: {traceId}");

            return BadRequest(new ErrorResponse
            {
                Error = validationResult.ErrorMessage ?? "Validation failed",
                TraceId = traceId
            });
        }

        logger.LogDebug($"{LogPrefix} - Validation successful, starting conversion, TraceId: {traceId}");
        return null;
    }
}
