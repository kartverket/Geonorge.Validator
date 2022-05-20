using System;
using System.Collections.Generic;

namespace Geonorge.XsdValidator.Models
{
    public class XsdValidatorResult
    {
        public List<string> Messages { get; set; }
        public Dictionary<string, Uri> CodelistUris { get; set; } = new();
    }
}
