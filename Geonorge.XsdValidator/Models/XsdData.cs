using System;
using System.Collections.Generic;
using System.IO;

namespace Geonorge.XsdValidator.Models
{
    public class XsdData
    {
        public List<Stream> Streams { get; set; } = new();
        public Uri BaseUri { get; set; }
    }
}
