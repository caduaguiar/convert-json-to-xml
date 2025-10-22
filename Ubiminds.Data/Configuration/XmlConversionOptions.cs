namespace Ubiminds.Data.Configuration;

public class XmlConversionOptions
{
    public const string SectionName = "XmlConversion";

    public string RootElementName { get; set; } = "Root";
    public bool IndentXml { get; set; } = true;
    public string IndentChars { get; set; } = "    ";
    public bool OmitXmlDeclaration { get; set; } = false;
    public string Encoding { get; set; } = "utf-8";
}
