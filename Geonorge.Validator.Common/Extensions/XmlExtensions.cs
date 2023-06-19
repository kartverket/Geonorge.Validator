using Geonorge.Validator.Common.Models;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Geonorge.Validator.Common.Extensions
{
    public static class XmlExtensions
    {
        public static XmlLineInfo ToXmlLineInfo(this XObject @object)
        {
            IXmlLineInfo info = @object;

            return info.HasLineInfo() ?
                new XmlLineInfo(info.LineNumber, info.LinePosition) :
                null;
        }

        public static XmlLineInfo ToXmlLineInfo(this XmlReader reader)
        {
            IXmlLineInfo info = (IXmlLineInfo)reader;

            return info.HasLineInfo() ?
                new XmlLineInfo(info.LineNumber, info.LinePosition) :
                null;
        }

        public static XmlSchemaLineInfo ToXmlSchemaLineInfo(this XmlSchemaElement schemaElement)
        {
            return new XmlSchemaLineInfo(schemaElement.LineNumber, schemaElement.LinePosition, schemaElement.SourceUri);
        }
    }
}
