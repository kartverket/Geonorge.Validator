using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using Geonorge.Validator.XmlSchema.Translator;
using System.Collections.Generic;
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
            InputData inputData, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);

            return await Validate(inputData, xmlReaderSettings);
        }

        private async Task<XmlSchemaValidatorResult> Validate(InputData inputData, XmlReaderSettings xmlReaderSettings)
        {
            using var memoryStream = await CopyStreamAsync(inputData.Stream);
            using var reader = XmlReader.Create(memoryStream, xmlReaderSettings);
            var schemaElements = new HashSet<XmlSchemaElement>();

            try
            {
                while (reader.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;

                    var schemaElement = reader.SchemaInfo.SchemaElement;

                    if (reader.NodeType != XmlNodeType.Element || schemaElement == null)
                        continue;

                    schemaElements.Add(schemaElement);
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

            return new XmlSchemaValidatorResult(_messages, schemaElements);
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
    }
}
