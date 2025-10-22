using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ubiminds.Data.Configuration;
using Ubiminds.Data.Extensions;

namespace Ubiminds.Data.Services;

public class PublishedItemXmlBuilder(
    ILogger<PublishedItemXmlBuilder> logger,
    IOptions<XmlConversionOptions> options) : IPublishedItemXmlBuilder
{
    private static readonly FrozenDictionary<string, string> JsonToXmlMapping = new Dictionary<string, string>
    {
        ["Title"] = "Title",
        ["CountryIds"] = "Countries",
        ["PublishDate"] = "PublishedDate",
        ["ReportMetadata"] = "ReportMetadata",
        ["ContactSection"] = "ContactSection",
        ["ContactInformation"] = "ContactInformation",
        ["ContactHeader"] = "Name",
        ["Contacts"] = "Contacts",
        ["FirstName"] = "GivenName",
        ["LastName"] = "FamilyName",
        ["PhoneNumber"] = "Number"
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> XmlElements = new Dictionary<string, string>
    {
        ["JobTitle"] = "JobTitle",
        ["PersonGroup"] = "PersonGroup",
        ["PersonGroupMember"] = "PersonGroupMember",
        ["Person"] = "Person",
        ["DisplayName"] = "DisplayName",
        ["ContactInfo"] = "ContactInfo",
        ["Phone"] = "Phone",
        ["ContactInformation"] = "ContactInformation"
    }.ToFrozenDictionary();

    private readonly XmlConversionOptions _options = options.Value;

    public string BuildXml(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);

        LogInformation(nameof(BuildXml), "Starting XML construction");

        try
        {
            var xmlContent = GenerateXmlContent(jsonDocument.RootElement);

            LogInformation(nameof(BuildXml), $"XML construction completed successfully with {xmlContent.Length} characters");

            return xmlContent;
        }
        catch (Exception ex)
        {
            LogError(nameof(BuildXml), ex, "Error during XML construction");
            throw new InvalidOperationException($"Failed to build XML from JSON: {ex.Message}", ex);
        }
    }

    private string GenerateXmlContent(JsonElement root)
    {
        var xmlSettings = CreateXmlWriterSettings();

        using var stringWriter = new Utf8StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings))
        {
            WriteRootElement(xmlWriter, root);
        }

        return stringWriter.ToString();
    }

    private XmlWriterSettings CreateXmlWriterSettings()
    {
        return new XmlWriterSettings
        {
            Indent = _options.IndentXml,
            IndentChars = _options.IndentChars,
            OmitXmlDeclaration = _options.OmitXmlDeclaration,
            Encoding = ParseEncoding(_options.Encoding)
        };
    }

    private void WriteRootElement(XmlWriter writer, JsonElement root)
    {
        writer.WriteStartElement(_options.RootElementName);

        WriteTitle(writer, root);
        WriteCountries(writer, root);
        WritePublishedDate(writer, root);
        WriteContactInformation(writer, root);

        writer.WriteEndElement();
    }

    private void WriteTitle(XmlWriter writer, JsonElement root)
    {
        var title = ExtractStringProperty(root, "Title");
        if (string.IsNullOrEmpty(title))
        {
            return;
        }

        writer.WriteElementString(JsonToXmlMapping["Title"], title);
        LogDebug(nameof(WriteTitle), "Title element written");
    }

    private void WriteCountries(XmlWriter writer, JsonElement root)
    {
        writer.WriteStartElement(JsonToXmlMapping["CountryIds"]);

        var countries = ExtractCountryList(root);
        if (countries.Count > 0)
        {
            writer.WriteString(string.Join(", ", countries));
            LogDebug(nameof(WriteCountries), $"Wrote {countries.Count} countries");
        }

        writer.WriteEndElement();
    }

    private static List<string> ExtractCountryList(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var countryIdsProperty = root.GetPropertyOrDefault("CountryIds");
        if (countryIdsProperty.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return countryIdsProperty
            .EnumerateArray()
            .Select(country => country.GetString())
            .Where(countryValue => !string.IsNullOrWhiteSpace(countryValue))
            .ToList()!;
    }

    private void WritePublishedDate(XmlWriter writer, JsonElement root)
    {
        var publishDate = ExtractStringProperty(root, "PublishDate");
        if (string.IsNullOrEmpty(publishDate))
        {
            return;
        }

        writer.WriteElementString(JsonToXmlMapping["PublishDate"], publishDate);
        LogDebug(nameof(WritePublishedDate), "PublishedDate element written");
    }

    private void WriteContactInformation(XmlWriter writer, JsonElement root)
    {
        writer.WriteStartElement(XmlElements["ContactInformation"]);

        var contactSections = ExtractContactSections(root);
        var personGroupSequence = 1;

        foreach (var contactSection in contactSections)
        {
            var validationResult = ValidateContactSection(contactSection);
            if (validationResult.IsValid)
            {
                WritePersonGroup(writer, validationResult.GroupName, validationResult.Contacts, personGroupSequence);
                personGroupSequence++;
                LogDebug(nameof(WriteContactInformation), $"Processed contact section: {validationResult.GroupName}");
            }
        }

        writer.WriteEndElement();
    }

    private static List<JsonElement> ExtractContactSections(JsonElement root)
    {
        var reportMetadata = root.GetPropertyOrDefault("ReportMetadata");
        if (reportMetadata.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var contactSection = reportMetadata.GetPropertyOrDefault("ContactSection");
        if (contactSection.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return contactSection
            .EnumerateArray()
            .Select(section => section.GetPropertyOrDefault("ContactInformation"))
            .Where(contactInformation => contactInformation.ValueKind == JsonValueKind.Array)
            .SelectMany(contactInformation => contactInformation.EnumerateArray())
            .ToList();
    }

    private static ContactSectionValidation ValidateContactSection(JsonElement contactSection)
    {
        var groupName = ExtractStringProperty(contactSection, "ContactHeader");
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return ContactSectionValidation.Invalid;
        }

        var contacts = contactSection.GetPropertyOrDefault("Contacts");
        if (contacts.ValueKind != JsonValueKind.Array)
        {
            return ContactSectionValidation.Invalid;
        }

        return new ContactSectionValidation(groupName, contacts);
    }

    private static void WritePersonGroup(XmlWriter writer, string groupName, JsonElement contacts, int sequence)
    {
        writer.WriteStartElement(XmlElements["PersonGroup"]);
        writer.WriteAttributeString("sequence", sequence.ToString());
        writer.WriteElementString(JsonToXmlMapping["ContactHeader"], groupName);

        foreach (var contact in contacts.EnumerateArray())
        {
            var person = ExtractPersonData(contact);
            if (person.IsValid)
            {
                WritePersonGroupMember(writer, person);
            }
        }

        writer.WriteEndElement();
    }

    private static PersonData ExtractPersonData(JsonElement contact)
    {
        return new PersonData
        {
            FirstName = ExtractStringProperty(contact, "FirstName"),
            LastName = ExtractStringProperty(contact, "LastName"),
            JobTitle = ExtractStringProperty(contact, "Title"),
            PhoneNumber = ExtractStringProperty(contact, "PhoneNumber")
        };
    }

    private static void WritePersonGroupMember(XmlWriter writer, PersonData person)
    {
        writer.WriteStartElement(XmlElements["PersonGroupMember"]);
        writer.WriteStartElement(XmlElements["Person"]);

        WritePersonName(writer, person);
        WritePersonJobTitle(writer, person.JobTitle);
        WritePersonContactInfo(writer, person.PhoneNumber);

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WritePersonName(XmlWriter writer, PersonData person)
    {
        if (!string.IsNullOrWhiteSpace(person.LastName))
        {
            writer.WriteElementString(JsonToXmlMapping["LastName"], person.LastName);
        }

        if (!string.IsNullOrWhiteSpace(person.FirstName))
        {
            writer.WriteElementString(JsonToXmlMapping["FirstName"], person.FirstName);
        }

        var displayName = person.GetDisplayName();
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            writer.WriteElementString(XmlElements["DisplayName"], displayName);
        }
    }

    private static void WritePersonJobTitle(XmlWriter writer, string jobTitle)
    {
        if (!string.IsNullOrWhiteSpace(jobTitle))
        {
            writer.WriteElementString(XmlElements["JobTitle"], jobTitle);
        }
    }

    private static void WritePersonContactInfo(XmlWriter writer, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return;
        }

        writer.WriteStartElement(XmlElements["ContactInfo"]);
        writer.WriteStartElement(XmlElements["Phone"]);
        writer.WriteElementString(JsonToXmlMapping["PhoneNumber"], phoneNumber);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string ExtractStringProperty(JsonElement element, string propertyName)
    {
        var property = element.GetPropertyOrDefault(propertyName);
        return property.ValueKind != JsonValueKind.Undefined
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static Encoding ParseEncoding(string encodingName)
    {
        return encodingName.ToLowerInvariant() switch
        {
            "utf-8" or "utf8" => Encoding.UTF8,
            "utf-16" or "utf16" or "unicode" => Encoding.Unicode,
            "ascii" => Encoding.ASCII,
            _ => Encoding.UTF8
        };
    }

    private void LogInformation(string methodName, string message) =>
        logger.LogInformationStructured(nameof(PublishedItemXmlBuilder), methodName, message);

    private void LogDebug(string methodName, string message) =>
        logger.LogDebugStructured(nameof(PublishedItemXmlBuilder), methodName, message);

    private void LogError(string methodName, Exception exception, string message) =>
        logger.LogErrorStructured(nameof(PublishedItemXmlBuilder), methodName, message, exception);

    private readonly struct ContactSectionValidation
    {
        public string GroupName { get; }
        public JsonElement Contacts { get; }
        public bool IsValid { get; }

        public static ContactSectionValidation Invalid => default;

        public ContactSectionValidation(string groupName, JsonElement contacts)
        {
            GroupName = groupName;
            Contacts = contacts;
            IsValid = true;
        }
    }

    private readonly struct PersonData
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string JobTitle { get; init; }
        public string PhoneNumber { get; init; }

        public bool IsValid => !string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName);

        public string GetDisplayName()
        {
            var firstName = FirstName?.Trim();
            var lastName = LastName?.Trim();

            return (firstName, lastName) switch
            {
                (null or "", null or "") => string.Empty,
                (var f, null or "") => f,
                (null or "", var l) => l,
                (var f, var l) => $"{f} {l}"
            };
        }
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
