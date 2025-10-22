using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ubiminds.Data.Configuration;
using Ubiminds.Data.Extensions;
using Ubiminds.Data.Models;

namespace Ubiminds.Data.Services;

public class ValidationService(
    ILogger<ValidationService> logger,
    IOptions<ValidationOptions> options) : IValidationService
{
    private static readonly Dictionary<string, string> FieldNames = new()
    {
        ["Status"] = "Status",
        ["PublishDate"] = "PublishDate",
        ["TestRun"] = "TestRun"
    };

    private readonly ValidationOptions _options = options.Value;

    public ValidationResult Validate(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);

        LogInformation(nameof(Validate), "Starting validation");

        var root = jsonDocument.RootElement;

        var statusValidation = ValidateStatus(root);
        if (!statusValidation.IsValid)
            return statusValidation;

        var publishDateValidation = ValidatePublishDate(root);
        if (!publishDateValidation.IsValid)
            return publishDateValidation;

        var testRunValidation = ValidateTestRun(root);
        if (!testRunValidation.IsValid)
            return testRunValidation;

        LogInformation(nameof(Validate), "Validation completed successfully");
        return ValidationResult.Success();
    }

    private ValidationResult ValidateStatus(JsonElement root)
    {
        var fieldName = FieldNames["Status"];

        if (!root.TryGetProperty(fieldName, out var statusElement))
        {
            LogWarning(nameof(ValidateStatus), "Status field is missing");
            return ValidationResult.Failure($"{fieldName} field is required");
        }

        if (statusElement.ValueKind != JsonValueKind.Number)
        {
            LogWarning(nameof(ValidateStatus), $"Status is not a number, type: {statusElement.ValueKind}");
            return ValidationResult.Failure($"{fieldName} must be a number");
        }

        var statusValue = statusElement.GetInt32();
        if (statusValue == _options.RequiredStatus) return ValidationResult.Success();

        LogWarning(nameof(ValidateStatus), $"Status validation failed, value: {statusValue}, expected: {_options.RequiredStatus}");
        return ValidationResult.Failure($"{fieldName} must be equal to {_options.RequiredStatus}");

    }

    private ValidationResult ValidatePublishDate(JsonElement root)
    {
        var fieldName = FieldNames["PublishDate"];

        if (!root.TryGetProperty(fieldName, out var publishDateElement))
        {
            LogWarning(nameof(ValidatePublishDate), "PublishDate field is missing");
            return ValidationResult.Failure($"{fieldName} field is required");
        }

        if (publishDateElement.ValueKind != JsonValueKind.String)
        {
            LogWarning(nameof(ValidatePublishDate), $"PublishDate is not a string, type: {publishDateElement.ValueKind}");
            return ValidationResult.Failure($"{fieldName} must be a valid date string");
        }

        var dateString = publishDateElement.GetString();
        if (string.IsNullOrWhiteSpace(dateString))
        {
            LogWarning(nameof(ValidatePublishDate), "PublishDate is empty or whitespace");
            return ValidationResult.Failure($"{fieldName} cannot be empty");
        }

        if (!DateTime.TryParse(dateString, out var publishDate))
        {
            LogWarning(nameof(ValidatePublishDate), $"PublishDate format is invalid: {dateString}");
            return ValidationResult.Failure($"{fieldName} is not in a valid date format");
        }

        var publishDateUtc = publishDate.Kind == DateTimeKind.Utc
            ? publishDate
            : publishDate.ToUniversalTime();

        var minPublishDateUtc = _options.MinPublishDate.Kind == DateTimeKind.Utc
            ? _options.MinPublishDate
            : _options.MinPublishDate.ToUniversalTime();

        if (publishDateUtc >= minPublishDateUtc) return ValidationResult.Success();

        LogWarning(nameof(ValidatePublishDate), $"PublishDate {publishDateUtc} is before minimum date {minPublishDateUtc}");
        return ValidationResult.Failure($"{fieldName} must be on or after {minPublishDateUtc:MM/dd/yyyy}");

    }

    private ValidationResult ValidateTestRun(JsonElement root)
    {
        var fieldName = FieldNames["TestRun"];

        if (!root.TryGetProperty(fieldName, out var testRunElement))
        {
            LogWarning(nameof(ValidateTestRun), "TestRun field is missing");
            return ValidationResult.Failure($"{fieldName} field is required");
        }

        if (testRunElement.ValueKind == JsonValueKind.True)
        {
            LogWarning(nameof(ValidateTestRun), "TestRun must be false, current value: true");
            return ValidationResult.Failure($"{fieldName} must be false");
        }

        if (testRunElement.ValueKind == JsonValueKind.False) return ValidationResult.Success();

        LogWarning(nameof(ValidateTestRun), $"TestRun is not a boolean, type: {testRunElement.ValueKind}");
        return ValidationResult.Failure($"{fieldName} must be a boolean value");

    }

    private void LogInformation(string methodName, string message) =>
        logger.LogInformationStructured(nameof(ValidationService), methodName, message);

    private void LogWarning(string methodName, string message) =>
        logger.LogWarningStructured(nameof(ValidationService), methodName, message);
}
