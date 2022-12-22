using System.Collections.Generic;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaValidatorResult
    {
        public XmlSchemaValidatorResult(
            List<XmlSchemaValidationError> messages, HashSet<XmlSchemaElement> schemaElements)
        {
            Messages = messages;
            SchemaElements = schemaElements;
        }

        public List<XmlSchemaValidationError> Messages { get; private set; }
        public HashSet<XmlSchemaElement> SchemaElements { get; private set; }
    }
}
