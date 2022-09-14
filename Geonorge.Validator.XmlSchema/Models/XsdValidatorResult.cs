using System;
using System.Collections.Generic;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XsdValidatorResult
    {
        public XsdValidatorResult(List<XmlSchemaValidationError> messages)
        {
            Messages = messages;
        }

        public XsdValidatorResult(List<XmlSchemaValidationError> messages, Dictionary<string, Uri> codelistUris)
        {
            Messages = messages;
            CodelistUris = codelistUris;
        }

        public List<XmlSchemaValidationError> Messages { get; set; }
        public Dictionary<string, Uri> CodelistUris { get; set; } = new();
    }
}
