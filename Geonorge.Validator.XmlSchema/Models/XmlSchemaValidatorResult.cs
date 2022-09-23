using System;
using System.Collections.Generic;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaValidatorResult
    {
        public XmlSchemaValidatorResult(List<XmlSchemaValidationError> messages)
        {
            Messages = messages;
        }

        public XmlSchemaValidatorResult(List<XmlSchemaValidationError> messages, Dictionary<string, Uri> codelistUris)
        {
            Messages = messages;
            CodelistUris = codelistUris;
        }

        public List<XmlSchemaValidationError> Messages { get; set; }
        public Dictionary<string, Uri> CodelistUris { get; set; } = new();
    }
}
