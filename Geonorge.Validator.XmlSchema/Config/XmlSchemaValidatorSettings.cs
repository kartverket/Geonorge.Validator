using Geonorge.Validator.XmlSchema.Validator;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Config
{
    public class XmlSchemaValidatorSettings
    {
        public static readonly string SectionName = "XmlSchemaValidator";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] CacheableHosts { get; set; }
        public int MaxMessageCount { get; set; }
    }
}
