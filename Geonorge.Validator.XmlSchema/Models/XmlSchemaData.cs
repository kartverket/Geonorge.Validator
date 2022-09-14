using System;
using System.Collections.Generic;
using System.IO;

namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaData
    {
        public List<Stream> Streams { get; set; } = new();
        public Uri BaseUri { get; set; }
    }
}
