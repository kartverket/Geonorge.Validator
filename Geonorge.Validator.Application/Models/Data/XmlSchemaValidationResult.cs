using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlSchemaValidationResult
    {
        public XmlSchemaRule Rule { get; set; }
        public Dictionary<string, Uri> CodelistUris { get; set; } = new();
    }
}
