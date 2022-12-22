using System.Collections.Generic;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlSchemaValidationResult
    {
        public XmlSchemaValidationResult(
            XmlSchemaRule rule, HashSet<XmlSchemaElement> xmlSchemaElements, XmlSchemaSet xmlSchemaSet)
        {
            Rule = rule;
            XmlSchemaElements = xmlSchemaElements;
            XmlSchemaSet = xmlSchemaSet;
        }

        public XmlSchemaRule Rule { get; private set; }
        public HashSet<XmlSchemaElement> XmlSchemaElements { get; set; }
        public XmlSchemaSet XmlSchemaSet { get; private set; }
    }
}
