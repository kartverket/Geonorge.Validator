using Geonorge.Validator.XmlSchema.Models;
using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlSchemaValidationResult
    {
        public XmlSchemaValidationResult(
            XmlSchemaRule rule, Dictionary<string, Uri> codelistUris, Dictionary<string, List<XLinkElement>> xLinkElements, XmlSchemaSet xmlSchemaSet)
        {
            Rule = rule;
            CodelistUris = codelistUris;
            XLinkElements = xLinkElements;
            XmlSchemaSet = xmlSchemaSet;
        }

        public XmlSchemaRule Rule { get; private set; }
        public Dictionary<string, Uri> CodelistUris { get; private set; }
        public Dictionary<string, List<XLinkElement>> XLinkElements { get; private set; }
        public XmlSchemaSet XmlSchemaSet { get; private set; }
    }
}
