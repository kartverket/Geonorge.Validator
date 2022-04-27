using Geonorge.Validator.Application.Extensions;
using Geonorge.XsdValidator.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.Validator.Application.Utils.XmlHelper;
using static Geonorge.XsdValidator.Utils.XsdHelper;

namespace Geonorge.Validator.Application.Utils.Codelist
{
    public class XsdCodelistExtractor : IXsdCodelistExtractor
    {
        private readonly XsdValidatorSettings _settings;

        public XsdCodelistExtractor(
            IOptions<XsdValidatorSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<Dictionary<string, Uri>> GetCodelistUrisAsync(
            Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            var xsdDocument = await LoadXDocumentAsync(xsdStream, LoadOptions.SetLineInfo);
            var relevantCodelistSelectors = GetRelevantCodelistSelectors(xsdDocument, codelistSelectors);

            if (!relevantCodelistSelectors.Any())
                return new();

            var xmlReaderSettings = GetXmlReaderSettings(xsdStream);
            var codelistUris = new Dictionary<string, Uri>();

            foreach (var xmlStream in xmlStreams)
                codelistUris.Append(GetCodelistUris(xsdDocument, xmlReaderSettings, xmlStream, relevantCodelistSelectors));

            return codelistUris;
        }

        private XmlReaderSettings GetXmlReaderSettings(Stream xsdStream)
        {
            var xmlSchemaSet = CreateXmlSchemaSet(xsdStream, _settings);
            var xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            xmlReaderSettings.Schemas.Add(xmlSchemaSet);
            xmlReaderSettings.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;

            xsdStream.Position = 0;

            return xmlReaderSettings;
        }

        private static Dictionary<string, Uri> GetCodelistUris(
            XDocument xsdDocument, XmlReaderSettings xmlReaderSettings, Stream xmlStream, List<XsdCodelistSelector> codelistSelectors)
        {
            using var reader = XmlReader.Create(xmlStream, xmlReaderSettings);
            using var wrapper = new XmlReaderPathWrapper(reader);

            var codeListUris = new Dictionary<string, Uri>();

            while (wrapper.Read())
            {
                var schemaElement = reader.SchemaInfo.SchemaElement;

                if (schemaElement == null || reader.NodeType != XmlNodeType.Element || codeListUris.ContainsKey(wrapper.Path))
                    continue;

                var selector = codelistSelectors.SingleOrDefault(selector => selector.QualifiedName == schemaElement.SchemaTypeName);

                if (selector == null)
                    continue;

                var element = GetElementAtLine(xsdDocument, schemaElement.LineNumber);

                if (element == null)
                    continue;

                var uriString = selector.UriResolver.Invoke(element);

                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                    continue;

                codeListUris.Add(wrapper.Path, uri);
            }

            xmlStream.Position = 0;

            return codeListUris;
        }

        private static List<XsdCodelistSelector> GetRelevantCodelistSelectors(XDocument xsdDocument, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            var documentElements = xsdDocument.Descendants();

            return codelistSelectors
                .Where(selector =>
                {
                    XNamespace ns = selector.QualifiedName.Namespace;
                    var prefix = xsdDocument.Root.GetPrefixOfNamespace(ns);
                    var type = $"{prefix}:{selector.QualifiedName.Name}";

                    return documentElements
                        .Any(element => element.Attribute("type")?.Value == type);
                })
                .ToList();
        }

        private static XElement GetElementAtLine(XDocument document, int lineNumber)
        {
            return document.Descendants()
                .SingleOrDefault(element => ((IXmlLineInfo)element).LineNumber == lineNumber);
        }
    }
}
