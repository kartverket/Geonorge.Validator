using Geonorge.XsdValidator.Models;
using Geonorge.XsdValidator.Translator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Validator
{
    internal class Validation
    {
        private readonly int _maxMessageCount;
        private readonly List<string> _messages;

        public Validation(int maxMessageCount = 1000)
        {
            _maxMessageCount = maxMessageCount;
            _messages = new List<string>(maxMessageCount);
        }

        public XsdValidatorResult Validate(Stream xmlStream, XmlSchemaSet xmlSchemaSet, Stream xsdStream, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);

            var xsdDocument = XDocument.Load(xsdStream, LoadOptions.SetLineInfo);
            xsdStream.Position = 0;
            
            var relevantCodelistSelectors = GetRelevantCodelistSelectors(xsdDocument, codelistSelectors);

            if (relevantCodelistSelectors.Any())
                return ValidateAndExtractCodelistUris(xmlStream, xmlReaderSettings, xsdDocument, relevantCodelistSelectors);
            
            return Validate(xmlStream, xmlReaderSettings);
        }

        private XsdValidatorResult Validate(Stream xmlStream, XmlReaderSettings xmlReaderSettings)
        {

            using var reader = XmlReader.Create(xmlStream, xmlReaderSettings);

            try
            {
                while (reader.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;
                }
            }
            catch (XmlException exception)
            {
                _messages.Add(MessageTranslator.TranslateError(exception.Message));
            }

            return new XsdValidatorResult { Messages = _messages };
        }

        private XsdValidatorResult ValidateAndExtractCodelistUris(
            Stream xmlStream, XmlReaderSettings xmlReaderSettings, XDocument xsdDocument, List<XsdCodelistSelector> codelistSelectors)
        {
            using var reader = XmlReader.Create(xmlStream, xmlReaderSettings);
            using var wrapper = new XmlReaderPathWrapper(reader);

            var codeListUris = new Dictionary<string, Uri>();

            try
            {
                while (wrapper.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;

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
            }
            catch (XmlException exception)
            {
                _messages.Add(MessageTranslator.TranslateError(exception.Message));
            }

            return new XsdValidatorResult { Messages = _messages, CodelistUris = codeListUris };
        }

        private XmlReaderSettings GetXmlReaderSettings(XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            xmlReaderSettings.Schemas.Add(xmlSchemaSet);            
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            xmlReaderSettings.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

            return xmlReaderSettings;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            var prefix = $"Linje {args.Exception.LineNumber}, posisjon {args.Exception.LinePosition}: ";

            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    prefix += MessageTranslator.TranslateError(args.Message);
                    break;
                case XmlSeverityType.Warning:
                    prefix += MessageTranslator.TranslateWarning(args.Message);
                    break;
            }

            _messages.Add(prefix);
        }

        private static XElement GetElementAtLine(XDocument document, int lineNumber)
        {
            return document.Descendants()
                .SingleOrDefault(element => ((IXmlLineInfo)element).LineNumber == lineNumber);
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
    }
}
