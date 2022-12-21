using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using Geonorge.Validator.XmlSchema.Translator;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.Validator.Common.Helpers.FileHelper;
using static Geonorge.Validator.Common.Helpers.XmlHelper;
using Wmhelp.XPath2;

namespace Geonorge.Validator.XmlSchema.Validator
{
    internal class Validation
    {
        private readonly string _cacheFilesPath;
        private readonly int _maxMessageCount;
        private readonly List<XmlSchemaValidationError> _messages;
        private bool _readingFailed = false;

        public Validation(string cacheFilesPath, int maxMessageCount = 1000)
        {
            _cacheFilesPath = cacheFilesPath;
            _maxMessageCount = maxMessageCount;
            _messages = new(maxMessageCount);
        }

        public async Task<XmlSchemaValidatorResult> Validate(
            InputData inputData, XmlSchemaSet xmlSchemaSet, Stream xmlSchemaStream, IEnumerable<XmlSchemaCodelistSelector> codelistSelectors)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);
            var mainXsdDocument = await LoadXDocumentAsync(xmlSchemaStream, LoadOptions.SetLineInfo);
            var relevantCodelistSelectors = GetRelevantCodelistSelectors(mainXsdDocument, codelistSelectors);

            if (relevantCodelistSelectors.Any())
                return await ValidateAndExtractCodelistUris(inputData, xmlReaderSettings, mainXsdDocument, relevantCodelistSelectors);

            return await Validate(inputData, xmlReaderSettings);
        }

        private async Task<XmlSchemaValidatorResult> Validate(InputData inputData, XmlReaderSettings xmlReaderSettings)
        {
            using var memoryStream = await CopyStreamAsync(inputData.Stream);
            using var reader = XmlReader.Create(memoryStream, xmlReaderSettings);
            var xLinkElements = new List<XLinkElement>();

            try
            {
                while (reader.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;

                    var schemaElement = reader.SchemaInfo.SchemaElement;

                    if (reader.NodeType != XmlNodeType.Element || schemaElement == null)
                        continue;

                    if (HasXLink(reader))
                    {
                        var lineInfo = (IXmlLineInfo)reader;                        

                        if (lineInfo.HasLineInfo())
                            xLinkElements.Add(XLinkElement.Create(lineInfo.LineNumber, lineInfo.LinePosition, schemaElement));
                    }
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

            return new XmlSchemaValidatorResult(_messages, xLinkElements);
        }

        public class SameXmlSchemaElement : EqualityComparer<XmlSchemaElement>
        {
            public override bool Equals(XmlSchemaElement x, XmlSchemaElement y)
            {
                if (x == null && y == null)
                    return true;
                else if (x == null || y == null)
                    return false;

                return x.LineNumber == y.LineNumber &&
                    x.LinePosition == y.LinePosition &&
                    x.SourceUri == y.SourceUri;
            }

            public override int GetHashCode([DisallowNull] XmlSchemaElement obj)
            {
                var b = $"{obj.LineNumber}-{obj.LinePosition}-{obj.SourceUri}";
                return b.GetHashCode();
            }
        }

        private async Task<XmlSchemaValidatorResult> ValidateAndExtractCodelistUris(
             InputData inputData, XmlReaderSettings xmlReaderSettings, XDocument mainXsdDocument, List<XmlSchemaCodelistSelector> codelistSelectors)
        {
            var s = DateTime.Now;

            using var memoryStream = await CopyStreamAsync(inputData.Stream);
            using var reader = XmlReader.Create(memoryStream, xmlReaderSettings);
            using var wrapper = new XmlReaderPathWrapper(reader);

            var codeListUris = new Dictionary<string, Uri>();
            var xLinkElements = new List<XLinkElement>();
            var xsdDocuments = new Dictionary<string, XDocument>();
            var schemaElements = new HashSet<XmlSchemaElement>();

            try
            {
                while (wrapper.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;

                    var schemaElement = reader.SchemaInfo.SchemaElement;

                    if (reader.NodeType != XmlNodeType.Element || schemaElement == null)
                        continue;

                    schemaElements.Add(schemaElement);

                    /*if (HasXLink(reader))
                    {
                        var lineInfo = (IXmlLineInfo)reader;

                        if (lineInfo.HasLineInfo())
                            xLinkElements.Add(XLinkElement.Create(lineInfo.LineNumber, lineInfo.LinePosition, schemaElement));
                    }

                    if (codeListUris.ContainsKey(wrapper.Path))
                        continue;

                    var selector = codelistSelectors.SingleOrDefault(selector => selector.QualifiedName == schemaElement.SchemaTypeName);

                    if (selector == null)
                        continue;

                    var xsdDocument = await GetXsdDocument(schemaElement, xsdDocuments) ?? mainXsdDocument;
                    var element = GetElementAtLine(xsdDocument, schemaElement.LineNumber);

                    if (element == null)
                        continue;

                    var uriString = selector.UriResolver.Invoke(element);

                    if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                        continue;

                    codeListUris.Add(wrapper.Path, uri);*/
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

            /*memoryStream.Position = 0;
            var doc = XDocument.Load(memoryStream, LoadOptions.SetLineInfo);

            var featureMembers = doc.XPath2SelectElements("//*:member/*").ToList();*/
            var qName = new XmlQualifiedName("CodeType", "http://www.opengis.net/gml/3.2");

            var info = new List<(XName, Uri)>();
            var codeElements = schemaElements
                .Where(ele => ele.SchemaTypeName.Equals(qName))
                .GroupBy(el => el.QualifiedName)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            var selector = codelistSelectors.SingleOrDefault(selector => selector.QualifiedName.Equals(qName));

            foreach (var (qualName, elements) in codeElements)
            {
                XNamespace xNs = qualName.Namespace;
                XName xName = xNs + qualName.Name;

                var uriStrings = new HashSet<string>();

                foreach (var element in elements)
                {
                    var xsdDocument = await GetXsdDocument(element, xsdDocuments) ?? mainXsdDocument;
                    var el = GetElementAtLine(xsdDocument, element.LineNumber);

                    if (el == null)
                        continue;

                    var uriString = selector.UriResolver.Invoke(el);

                    uriStrings.Add(uriString);
                }

                var uri = uriStrings
                    .Where(uriString => uriString != null)
                    .FirstOrDefault();

                if (uri != null && Uri.TryCreate(uri, UriKind.Absolute, out var uri2))
                    info.Add((xName, uri2));

            }

            /*foreach (var element in codeElements)
            {
                var qn = element.QualifiedName;
                XNamespace xNs = qn.Namespace;
                XName xName = xNs + qn.Name;

                var xsdDocument = await GetXsdDocument(element, xsdDocuments) ?? mainXsdDocument;
                var el = GetElementAtLine(xsdDocument, element.LineNumber);

                if (el == null)
                    continue;

                
                var uriString = selector.UriResolver.Invoke(el);

                info.Add((xName, uriString));
            }
            /*
            var codes = schemaElements.Where(ele => ele.SchemaTypeName == qName)
                .Select(ele =>
                {
                    var qn = ele.QualifiedName;
                    XNamespace xNs = qn.Namespace;
                    XName xName = xNs + qn.Name;


                    return xName;
                })
                .ToList();

            var a = featureMembers.Descendants().Where(element => codes.Any(name => name.Equals(element.Name))).ToList();
            */

            var e = DateTime.Now.Subtract(s).TotalSeconds;

            memoryStream.Position = 0;
            var doc = XDocument.Load(memoryStream, LoadOptions.SetLineInfo);

            var s1 = DateTime.Now;

            var featureMembers = doc.Root.Descendants()
                .Where(element => info.Any(kvp => kvp.Item1.Equals(element.Name)))
                .ToList();

            var e1 = DateTime.Now.Subtract(s1).TotalSeconds;

            await EnrichValidationErrors(inputData);

            return new XmlSchemaValidatorResult(_messages, codeListUris, xLinkElements);
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

        private static bool HasXLink(XmlReader reader)
        {
            if (!reader.HasAttributes)
                return false;

            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "xlink:href")
                {
                    reader.MoveToElement();
                    return true;
                }
            }

            reader.MoveToElement();
            return false;
        }

        private async Task<XDocument> GetXsdDocument(
            XmlSchemaElement schemaElement, Dictionary<string, XDocument> xsdDocuments)
        {
            if (schemaElement.SourceUri == "")
                return null;

            if (xsdDocuments.ContainsKey(schemaElement.SourceUri))
                return xsdDocuments[schemaElement.SourceUri];

            if (!Uri.TryCreate(schemaElement.SourceUri, UriKind.Absolute, out var uri))
                return null;

            var filePath = GetFilePath(uri);

            if (!File.Exists(filePath))
                return null;

            try
            {
                var xsdDocument = await LoadXDocumentAsync(File.OpenRead(filePath), LoadOptions.SetLineInfo);
                xsdDocuments.Add(schemaElement.SourceUri, xsdDocument);

                return xsdDocument;
            }
            catch
            {
                return null;
            }
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_cacheFilesPath, uri.Host + uri.LocalPath));
        }
    }
}
