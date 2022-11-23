using System.Xml;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XLinkElement
    {
        private static readonly XmlQualifiedName _referenceTypeName = new("ReferenceType", "http://www.opengis.net/gml/3.2");

        private XLinkElement(int lineNumber, int linePosition, XmlSchemaElement schemaElement, XLinkType type)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
            SchemaElement = schemaElement;
            Type = type;
        }

        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }
        public XmlSchemaElement SchemaElement { get; private set; }
        public XLinkType Type { get; private set; }

        public static XLinkElement Create(int lineNumber, int linePosition, XmlSchemaElement schemaElement)
        {
            var type = schemaElement.SchemaTypeName == _referenceTypeName ? XLinkType.Codelist : XLinkType.Object;

            return new XLinkElement(lineNumber, linePosition, schemaElement, type);
        }
    }
}
