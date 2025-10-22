namespace Ubiminds.Data.Configuration;

public class ValidationOptions
{
    public const string SectionName = "Validation";

    public int RequiredStatus { get; set; } = 3;
    public DateTime MinPublishDate { get; set; } = new(2024, 8, 24, 0, 0, 0, DateTimeKind.Utc);
}
