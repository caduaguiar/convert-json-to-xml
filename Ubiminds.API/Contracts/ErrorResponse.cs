namespace Ubiminds.API.Contracts;

public record ErrorResponse
{
    public required string Error { get; init; }
    public IEnumerable<string>? Details { get; init; }
    public string? TraceId { get; init; }
}