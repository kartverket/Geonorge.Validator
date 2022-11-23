using System;
using System.Collections.Generic;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaValidatorResult
    {
        public XmlSchemaValidatorResult(
            List<XmlSchemaValidationError> messages, List<XLinkElement> xLinkElements)
        {
            Messages = messages;
            XLinkElements = xLinkElements;
        }

        public XmlSchemaValidatorResult(
            List<XmlSchemaValidationError> messages, Dictionary<string, Uri> codelistUris, List<XLinkElement> xLinkElements)
        {
            Messages = messages;
            CodelistUris = codelistUris;
            XLinkElements = xLinkElements;
        }

        public List<XmlSchemaValidationError> Messages { get; private set; }
        public Dictionary<string, Uri> CodelistUris { get; private set; } = new();
        public List<XLinkElement> XLinkElements { get; private set; }
    }
}
