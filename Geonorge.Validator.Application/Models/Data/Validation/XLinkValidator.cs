using Geonorge.Validator.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using CodeList = Geonorge.Validator.Application.Models.Data.Codelist.Codelist;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class XLinkValidator
    {
        private static readonly Regex _xmlSchemaErrorRegex =
            new(@"^.*?invalid child element '(?<childElement>[^ ]*)' in namespace '(?<childNs>[^ ]*)'.( List of possible elements expected: '(?<posElements>.*?)' in namespace '(?<posNs>.*?)')*( as well as '(?<otherElements>.*?)' in namespace '(?<otherNs>.*?)')*", RegexOptions.Compiled);

        public XLinkValidator(
            XmlSchemaSet xmlSchemaSet,
            HashSet<XmlSchemaElement> xmlSchemaElements,
            Dictionary<string, Dictionary<XmlLineInfo, XmlSchemaLineInfo>> xmlSchemaMappings,
            Func<Uri, Task<CodeList>> fetchCodelist)
        {
            XmlSchemaSet = xmlSchemaSet;
            XmlSchemaElements = xmlSchemaElements;
            XmlSchemaMappings = xmlSchemaMappings;
            FetchCodelist = fetchCodelist;
        }

        public XmlSchemaSet XmlSchemaSet { get; }
        public HashSet<XmlSchemaElement> XmlSchemaElements { get; } = new();
        public Dictionary<string, Dictionary<XmlLineInfo, XmlSchemaLineInfo>> XmlSchemaMappings { get; } = new();
        public Func<Uri, Task<CodeList>> FetchCodelist { get; }

        public (string RefElement, string ValidElements) Validate(XElement element, XElement refElement, XmlSchemaElement xmlSchemaElement)
        {
            var clonedElement = new XElement(element);
            clonedElement.RemoveAttributes();
            clonedElement.Add(refElement);

            (string ReferenceElement, string PossibleElements) result = default;

            clonedElement.Validate(xmlSchemaElement, XmlSchemaSet, (object sender, ValidationEventArgs args) =>
            {
                if (args.Severity == XmlSeverityType.Error)
                {
                    result = ParseXmlSchemaError(args.Message, element);

                    if (result != default)
                        return;
                }
            });

            return result;
        }

        private static (string RefElement, string ValidElements) ParseXmlSchemaError(string message, XElement element)
        {
            var match = _xmlSchemaErrorRegex.Match(message);

            if (!match.Success)
                return default;

            var refElementNsPrefix = GetPrefixOfNamespace(GetMatchValue(match, "childNs"), element);
            var refElement = GetElementWithPrefix(refElementNsPrefix, GetMatchValue(match, "childElement"));

            var possibleNsPrefix = GetPrefixOfNamespace(GetMatchValue(match, "posNs"), element);
            var validElements = GetMatchValue(match, "posElements")?.Split(',', StringSplitOptions.TrimEntries)
                .Select(element => GetElementWithPrefix(possibleNsPrefix, element))
                .ToList();

            List<string[]> otherElements = new();
            List<string> otherNs = new();

            foreach (Group group in match.Groups)
            {
                if (group.Name == "otherElements" && group.Success)
                {
                    otherElements = group.Captures
                        .Select(capture => capture.Value.Split(',', StringSplitOptions.TrimEntries))
                        .ToList();
                }
                else if (group.Name == "otherNs" && group.Success)
                {
                    otherNs = group.Captures
                        .Select(capture => capture.Value)
                        .ToList();
                }
            }

            if (!otherElements.Any())
                return (refElement, string.Join(", ", validElements));

            for (int i = 0; i < otherElements.Count; i++)
            {
                var prefix = GetPrefixOfNamespace(otherNs.ElementAtOrDefault(i), element);

                for (int j = 0; j < otherElements[i].Length; j++)
                    validElements.Add(GetElementWithPrefix(prefix, otherElements[i][j]));
            }

            return (refElement, string.Join(", ", validElements));
        }

        private static string GetMatchValue(Match match, string groupName)
        {
            var value = match.Groups[groupName].Value;

            return !string.IsNullOrWhiteSpace(value) ? value : null;
        }

        private static string GetPrefixOfNamespace(string @namespace, XElement element)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                return null;

            XNamespace ns = @namespace;

            return element.GetPrefixOfNamespace(ns);
        }

        private static string GetElementWithPrefix(string prefix, string element)
        {
            return !string.IsNullOrWhiteSpace(prefix) ? $"{prefix}:{element}" : element;
        }
    }
}
