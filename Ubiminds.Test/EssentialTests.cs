using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Ubiminds.Data.Configuration;
using Ubiminds.Data.Models;
using Ubiminds.Data.Services;

namespace Ubiminds.Test;

public class EssentialTests
{
    [Fact]
    public void Should_ReturnSuccess_When_DocumentHasAllValidFields()
    {
        var logger = Substitute.For<ILogger<ValidationService>>();
        var options = Options.Create(new ValidationOptions
        {
            RequiredStatus = 3,
            MinPublishDate = new DateTime(2024, 8, 24, 0, 0, 0, DateTimeKind.Utc)
        });
        var validationService = new ValidationService(logger, options);

        var json = """
            {
                "Status": 3,
                "PublishDate": "2024-08-26T18:19:59Z",
                "TestRun": false
            }
            """;
        using var document = JsonDocument.Parse(json);

        var result = validationService.Validate(document);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Should_ReturnFailure_When_StatusIsNotEqualTo3()
    {
        var logger = Substitute.For<ILogger<ValidationService>>();
        var options = Options.Create(new ValidationOptions
        {
            RequiredStatus = 3,
            MinPublishDate = new DateTime(2024, 8, 24, 0, 0, 0, DateTimeKind.Utc)
        });
        var validationService = new ValidationService(logger, options);

        var json = """
            {
                "Status": 2,
                "PublishDate": "2024-08-26T18:19:59Z",
                "TestRun": false
            }
            """;
        using var document = JsonDocument.Parse(json);

        var result = validationService.Validate(document);

        Assert.False(result.IsValid);
        Assert.Equal("Status must be equal to 3", result.ErrorMessage);
    }

    [Fact]
    public void Should_ReturnFailure_When_PublishDateIsBeforeMinimumDate()
    {
        var logger = Substitute.For<ILogger<ValidationService>>();
        var options = Options.Create(new ValidationOptions
        {
            RequiredStatus = 3,
            MinPublishDate = new DateTime(2024, 8, 24, 0, 0, 0, DateTimeKind.Utc)
        });
        var validationService = new ValidationService(logger, options);

        var json = """
            {
                "Status": 3,
                "PublishDate": "2024-08-20T18:19:59Z",
                "TestRun": false
            }
            """;
        using var document = JsonDocument.Parse(json);

        var result = validationService.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains("PublishDate must be on or after 08/24/2024", result.ErrorMessage);
    }

    [Fact]
    public void Should_ReturnFailure_When_TestRunIsTrue()
    {
        var logger = Substitute.For<ILogger<ValidationService>>();
        var options = Options.Create(new ValidationOptions
        {
            RequiredStatus = 3,
            MinPublishDate = new DateTime(2024, 8, 24, 0, 0, 0, DateTimeKind.Utc)
        });
        var validationService = new ValidationService(logger, options);

        var json = """
            {
                "Status": 3,
                "PublishDate": "2024-08-26T18:19:59Z",
                "TestRun": true
            }
            """;
        using var document = JsonDocument.Parse(json);

        var result = validationService.Validate(document);

        Assert.False(result.IsValid);
        Assert.Equal("TestRun must be false", result.ErrorMessage);
    }

    [Fact]
    public void Should_GenerateWellFormedXml_When_ConvertingValidJson()
    {
        var converterLogger = Substitute.For<ILogger<JsonToXmlConverter>>();
        var builderLogger = Substitute.For<ILogger<PublishedItemXmlBuilder>>();
        var options = Options.Create(new XmlConversionOptions
        {
            RootElementName = "PublishedItem",
            IndentXml = true,
            IndentChars = "    ",
            OmitXmlDeclaration = false,
            Encoding = "utf-8"
        });
        var xmlBuilder = new PublishedItemXmlBuilder(builderLogger, options);
        var converter = new JsonToXmlConverter(converterLogger, xmlBuilder);

        var json = """
            {
                "Status": 3,
                "PublishDate": "2024-08-26T18:19:59Z",
                "TestRun": false,
                "Title": "Test Document"
            }
            """;
        using var document = JsonDocument.Parse(json);

        var xml = converter.Convert(document);

        Assert.NotNull(xml);
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml);
        Assert.Contains("<PublishedItem>", xml);
        Assert.Contains("<Title>Test Document</Title>", xml);
        Assert.Contains("</PublishedItem>", xml);

        var xmlDoc = XDocument.Parse(xml);
        Assert.NotNull(xmlDoc.Root);
    }

    [Fact]
    public void Should_PreserveNestedStructure_When_ConvertingComplexJson()
    {
        var converterLogger = Substitute.For<ILogger<JsonToXmlConverter>>();
        var builderLogger = Substitute.For<ILogger<PublishedItemXmlBuilder>>();
        var options = Options.Create(new XmlConversionOptions
        {
            RootElementName = "PublishedItem",
            IndentXml = true,
            IndentChars = "    ",
            OmitXmlDeclaration = false,
            Encoding = "utf-8"
        });
        var xmlBuilder = new PublishedItemXmlBuilder(builderLogger, options);
        var converter = new JsonToXmlConverter(converterLogger, xmlBuilder);

        var json = """
            {
                "Id": "TCnWpDVD",
                "Status": 3,
                "PublishDate": "2024-08-26T18:19:59Z",
                "TestRun": false,
                "CountryIds": ["US", "CA", "MX"],
                "Title": "Test Report",
                "ReportMetadata": {
                    "ContactSection": [
                        {
                            "ContactInformation": [
                                {
                                    "ContactHeader": "Media Contact",
                                    "Contacts": [
                                        {
                                            "FirstName": "Mike",
                                            "LastName": "Johnsen",
                                            "Email": "mike.johnsen@example.com",
                                            "PhoneNumber": "1-646-731-2332"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            }
            """;
        using var document = JsonDocument.Parse(json);

        var xml = converter.Convert(document);

        Assert.Contains("<PublishedItem>", xml);
        Assert.Contains("<Countries>US, CA, MX</Countries>", xml);
        Assert.Contains("<PersonGroup sequence=\"1\">", xml);
        Assert.Contains("<Name>Media Contact</Name>", xml);
        Assert.Contains("<GivenName>Mike</GivenName>", xml);
        Assert.Contains("<FamilyName>Johnsen</FamilyName>", xml);
        Assert.Contains("<Number>1-646-731-2332</Number>", xml);

        var xmlDoc = XDocument.Parse(xml);
        Assert.NotNull(xmlDoc.Root);
    }

    [Fact]
    public void Should_EscapeSpecialCharacters_When_ConvertingToXml()
    {
        var converterLogger = Substitute.For<ILogger<JsonToXmlConverter>>();
        var builderLogger = Substitute.For<ILogger<PublishedItemXmlBuilder>>();
        var options = Options.Create(new XmlConversionOptions
        {
            RootElementName = "PublishedItem",
            IndentXml = true,
            IndentChars = "    ",
            OmitXmlDeclaration = false,
            Encoding = "utf-8"
        });
        var xmlBuilder = new PublishedItemXmlBuilder(builderLogger, options);
        var converter = new JsonToXmlConverter(converterLogger, xmlBuilder);

        var json = """
            {
                "Status": 3,
                "Title": "Text with <special> & \"characters\"",
                "PublishDate": "2024-08-26T18:19:59Z"
            }
            """;
        using var document = JsonDocument.Parse(json);

        var xml = converter.Convert(document);

        Assert.Contains("&lt;", xml);
        Assert.Contains("&gt;", xml);
        Assert.Contains("&amp;", xml);

        var xmlDoc = XDocument.Parse(xml);
        Assert.NotNull(xmlDoc.Root);
    }
}
