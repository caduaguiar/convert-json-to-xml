using System.Text.Json;
using Ubiminds.Data.Models;

namespace Ubiminds.Data.Services;

public interface IValidationService
{
    ValidationResult Validate(JsonDocument jsonDocument);
}
