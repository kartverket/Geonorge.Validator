using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using Geonorge.Validator.XmlSchema.Translator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.Validator.Common.Helpers.FileHelper;
using static Geonorge.Validator.Common.Helpers.XmlHelper;

namespace Geonorge.Validator.XmlSchema.Validator
{
    internal class Validation
    {
        private readonly int _maxMessageCount;
        private readonly List<XmlSchemaValidationError> _messages;
        private bool _readingFailed = false;

        public Validation(int maxMessageCount = 1000)
        {
            _maxMessageCount = maxMessageCount;
            _messages = new(maxMessageCount);
        }

        public async Task<XmlSchemaValidatorResult> Validate(
            InputData inputData, XmlSchemaSet xmlSchemaSet, Stream xmlSchemaStream, IEnumerable<XmlSchemaCodelistSelector> codelistSelectors)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);
            var xsdDocument = await LoadXDocumentAsync(xmlSchemaStream, LoadOptions.SetLineInfo);
            var relevantCodelistSelectors = GetRelevantCodelistSelectors(xsdDocument, codelistSelectors);

            if (relevantCodelistSelectors.Any())
                return await ValidateAndExtractCodelistUris(inputData, xmlReaderSettings, xsdDocument, relevantCodelistSelectors);

            return await Validate(inputData, xmlReaderSettings);
        }

        private async Task<XmlSchemaValidatorResult> Validate(InputData inputData, XmlReaderSettings xmlReaderSettings)
        {
            using var memoryStream = await CopyStreamAsync(inputData.Stream);
            using var reader = XmlReader.Create(memoryStream, xmlReaderSettings);

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
                var message = new XmlSchemaValidationError
                {
                    Message = MessageTranslator.TranslateError(exception.Message),
                    LineNumber = exception.LineNumber,
                    LinePosition = exception.LinePosition,
                    FileName = inputData.FileName
                };

                _messages.Add(message);
                _readingFailed = true;
            }

            await EnrichValidationErrors(inputData);

            return new XmlSchemaValidatorResult(_messages);
        }

        private async Task<XmlSchemaValidatorResult> ValidateAndExtractCodelistUris(
            InputData inputData, XmlReaderSettings xmlReaderSettings, XDocument xsdDocument, List<XmlSchemaCodelistSelector> codelistSelectors)
        {
            using var memoryStream = await CopyStreamAsync(inputData.Stream);
            using var reader = XmlReader.Create(memoryStream, xmlReaderSettings);
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
                var message = new XmlSchemaValidationError
                {
                    Message = MessageTranslator.TranslateError(exception.Message),
                    LineNumber = exception.LineNumber,
                    LinePosition = exception.LinePosition,
                    FileName = inputData.FileName
                };

                _messages.Add(message);
                _readingFailed = true;
            }

            await EnrichValidationErrors(inputData);

            return new XmlSchemaValidatorResult(_messages, codeListUris);
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

            _messages.Add(new XmlSchemaValidationError
            {
                Message = prefix,
                LineNumber = args.Exception.LineNumber,
                LinePosition = args.Exception.LinePosition
            });
        }

        private async Task EnrichValidationErrors(InputData inputData)
        {
            if (!_messages.Any())
                return;

            XDocument document = null;

            if (!_readingFailed)
                document = await LoadXDocumentAsync(inputData.Stream, LoadOptions.SetLineInfo);

            foreach (var message in _messages)
            {
                if (document != null)
                {
                    var element = GetElementAtLine(document, message.LineNumber);

                    if (element != null)
                        message.XPath = element.GetXPath();
                }

                message.FileName = inputData.FileName;
            }
        }

        private static XElement GetElementAtLine(XDocument document, int lineNumber)
        {
            return document.Descendants()
                .SingleOrDefault(element => ((IXmlLineInfo)element).LineNumber == lineNumber);
        }

        private static List<XmlSchemaCodelistSelector> GetRelevantCodelistSelectors(XDocument xsdDocument, IEnumerable<XmlSchemaCodelistSelector> codelistSelectors)
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
