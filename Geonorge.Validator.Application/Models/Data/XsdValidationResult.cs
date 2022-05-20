using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XsdValidationResult
    {
        public XsdRule Rule { get; set; }
        public Dictionary<string, Uri> CodelistUris { get; set; } = new();
    }
}
