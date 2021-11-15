using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Translator;
using Geonorge.XsdValidator.Utils;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Validator
{
    internal class ValidatorRun
    {
        private const int ValidationErrorCountLimit = 1000;
        private readonly List<string> _schemaValidationResult;
        private readonly XsdValidatorSettings _options;

        public ValidatorRun(XsdValidatorSettings options)
        {
            _options = options;
            _schemaValidationResult = new List<string>();
        }

        public List<string> Validate(Stream xmlStream, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);

            Validate(xmlStream, xmlReaderSettings);

            return _schemaValidationResult;
        }

        private void Validate(Stream xmlStream, XmlReaderSettings xmlReaderSettings)
        {
            using var validationReader = XmlReader.Create(xmlStream, xmlReaderSettings);

            try
            {
                while (validationReader.Read())
                    if (_schemaValidationResult.Count >= ValidationErrorCountLimit)
                        break;
            }
            catch (XmlException exception)
            {
                _schemaValidationResult.Add(MessageTranslator.TranslateError(exception.Message));
            }
        }

        private XmlReaderSettings GetXmlReaderSettings(XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            xmlReaderSettings.XmlResolver = new XmlFileCacheResolver(_options);

            if (xmlSchemaSet != null)
                xmlReaderSettings.Schemas.Add(xmlSchemaSet);
            else
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;

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

            _schemaValidationResult.Add(prefix);
        }
    }
}
