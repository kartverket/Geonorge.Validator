using Geonorge.Validator.Common.Models;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlSchemaValidationResult
    {
        public XmlSchemaValidationResult(
            XmlSchemaRule rule, 
            HashSet<XmlSchemaElement> xmlSchemaElements, 
            Dictionary<string, Dictionary<XmlLineInfo, XmlSchemaLineInfo>> xmlSchemaMappings, 
            XmlSchemaSet xmlSchemaSet)
        {
            Rule = rule;
            XmlSchemaElements = xmlSchemaElements;
            XmlSchemaMappings = xmlSchemaMappings;
            XmlSchemaSet = xmlSchemaSet;
        }

        public XmlSchemaRule Rule { get; private set; }
        public HashSet<XmlSchemaElement> XmlSchemaElements { get; set; }
        public Dictionary<string, Dictionary<XmlLineInfo, XmlSchemaLineInfo>> XmlSchemaMappings { get; set; }
        public XmlSchemaSet XmlSchemaSet { get; private set; }
    }
}
