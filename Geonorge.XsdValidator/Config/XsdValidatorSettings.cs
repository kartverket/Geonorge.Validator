using Geonorge.XsdValidator.Validator;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Geonorge.XsdValidator.Config
{
    public class XsdValidatorSettings
    {
        public static readonly string SectionName = "XsdValidator";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] CacheableHosts { get; set; }
        public int MaxMessageCount { get; set; }
        public List<XsdCodelistSelector> CodelistSelectors { get; } = new();

        public void AddCodelistSelector(XNamespace @namespace, string elementName, Func<XElement, string> uriResolver)
        {
            CodelistSelectors.Add(new XsdCodelistSelector(@namespace, elementName, uriResolver));
        }
    }
}
