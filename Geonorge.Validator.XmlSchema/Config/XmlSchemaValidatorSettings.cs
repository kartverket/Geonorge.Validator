using Geonorge.Validator.XmlSchema.Validator;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Geonorge.Validator.XmlSchema.Config
{
    public class XmlSchemaValidatorSettings
    {
        public static readonly string SectionName = "XmlSchemaValidator";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] CacheableHosts { get; set; }
        public int MaxMessageCount { get; set; }
        public List<XmlSchemaCodelistSelector> CodelistSelectors { get; } = new();
        public HashSet<string> IgnoredNamespaces { get; } = new();

        public void AddCodelistSelector(XName elementName, Func<XElement, string> uriResolver)
        {
            CodelistSelectors.Add(new XmlSchemaCodelistSelector(elementName, uriResolver));
        }

        public void IgnoreNamespace(string xmlNamespace)
        {
            IgnoredNamespaces.Add(xmlNamespace);
        }
    }
}
