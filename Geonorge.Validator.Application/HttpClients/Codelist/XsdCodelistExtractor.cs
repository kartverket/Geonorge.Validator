using Geonorge.Validator.Application.Exceptions;
using Geonorge.XsdValidator.Config;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.XsdValidator.Utils.XsdHelper;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class XsdCodelistExtractor : IXsdCodelistExtractor
    {
        private readonly XsdValidatorSettings _settings;

        public XsdCodelistExtractor(
            IOptions<XsdValidatorSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<Dictionary<string, Uri>> GetCodelistsFromXsd(Stream xmlStream, Stream xsdStream, List<CodelistSelector> codelistSelectors)
        {
            var xsdDocument = await LoadXDocumentAsync(xsdStream);
            var xmlReaderSettings = GetXmlReaderSettings(xsdStream);

            using var reader = XmlReader.Create(xmlStream, xmlReaderSettings);
            using var wrapper = new XmlReaderPathWrapper(reader);

            var codeLists = new Dictionary<string, Uri>();

            while (wrapper.Read())
            {
                var schemaElement = reader.SchemaInfo.SchemaElement;

                if (schemaElement == null || reader.NodeType != XmlNodeType.Element || codeLists.ContainsKey(wrapper.Path))
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

                codeLists.Add(wrapper.Path, uri);
            }

            xmlStream.Position = 0;

            return codeLists;
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

        private static XElement GetElementAtLine(XDocument document, int lineNumber)
        {
            return document.Descendants()
                .SingleOrDefault(element => ((IXmlLineInfo)element).LineNumber == lineNumber);
        }

        private static async Task<XDocument> LoadXDocumentAsync(Stream xsdStream)
        {
            try
            {
                var document = await XDocument.LoadAsync(xsdStream, LoadOptions.SetLineInfo, new CancellationToken());
                xsdStream.Position = 0;

                return document;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Ugyldig applikasjonsskjema");
                throw new InvalidXsdException($"Ugyldig applikasjonsskjema");
            }
        }
    }
}
