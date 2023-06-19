using Geonorge.Validator.Common.Models;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaValidatorResult
    {
        public XmlSchemaValidatorResult(
            List<XmlSchemaValidationError> messages, HashSet<XmlSchemaElement> schemaElements, Dictionary<XmlLineInfo, XmlSchemaLineInfo> schemaMappings)
        {
            Messages = messages;
            SchemaElements = schemaElements;
            SchemaMappings = schemaMappings;
        }

        public List<XmlSchemaValidationError> Messages { get; private set; }
        public HashSet<XmlSchemaElement> SchemaElements { get; private set; }
        public Dictionary<XmlLineInfo, XmlSchemaLineInfo> SchemaMappings { get; set; }
    }
}
