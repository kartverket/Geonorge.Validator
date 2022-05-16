using System;
using System.IO;

namespace Geonorge.XsdValidator.Models
{
    public class XsdData
    {
        public Stream Stream { get; set; }
        public Uri BaseUri { get; set; }
    }
}
