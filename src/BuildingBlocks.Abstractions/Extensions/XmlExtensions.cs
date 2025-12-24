using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace BuildingBlocks.Abstractions.Extensions;

public static class XmlExtensions
{
    public static string ToXml<T>(this T obj, Encoding? encoding = null, bool omitXmlDeclaration = false)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        encoding ??= Encoding.UTF8;

        var serializer = new XmlSerializer(typeof(T));

        var settings = new XmlWriterSettings
        {
            Encoding = encoding,
            OmitXmlDeclaration = omitXmlDeclaration,
            Indent = true
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        serializer.Serialize(xmlWriter, obj);
        return stringWriter.ToString();
    }

    public static T FromXml<T>(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML string cannot be null or empty.", nameof(xml));

        var serializer = new XmlSerializer(typeof(T));

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);

        var result = serializer.Deserialize(xmlReader);
        return (T)result!;
    }

    public static bool TryFromXml<T>(this string xml, out T? result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            result = xml.FromXml<T>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidXml(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return false;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string FormatXml(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return xml;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            doc.Save(xmlWriter);
            return stringWriter.ToString();
        }
        catch
        {
            return xml; // Return original if formatting fails
        }
    }

    public static string RemoveNamespaces(this string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return xml;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            // Remove namespace declarations
            var elementsToUpdate = doc.SelectNodes("//*")?.Cast<XmlElement>().ToList();
            if (elementsToUpdate != null)
            {
                foreach (var element in elementsToUpdate)
                {
                    element.RemoveAllAttributes();
                    if (element.NamespaceURI != string.Empty)
                    {
                        var newElement = doc.CreateElement(element.LocalName);
                        newElement.InnerXml = element.InnerXml;
                        element.ParentNode?.ReplaceChild(newElement, element);
                    }
                }
            }

            return doc.OuterXml;
        }
        catch
        {
            return xml; // Return original if processing fails
        }
    }
}